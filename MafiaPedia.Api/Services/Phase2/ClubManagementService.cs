using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Club;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubManagementService : IClubManagementService
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ClubManagementService(MafiaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<List<ClubDto>> GetAllClubsAsync()
    {
        return await _context.Clubs
            .OrderBy(c => c.Name)
            .Select(c => new ClubDto(
                c.Id, c.Name ?? "", c.Address, c.Phone, c.City, c.Description, c.Logo
            ))
            .ToListAsync();
    }

    public async Task<ClubDetailDto?> GetClubDetailAsync(int clubId)
    {
        var club = await _context.Clubs
            .Include(c => c.Rooms)
            .Include(c => c.Masters)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == clubId);

        if (club is null) return null;

        return new ClubDetailDto(
            club.Id,
            club.Name ?? "",
            club.Address,
            club.Phone,
            club.City,
            club.Description,
            club.Logo,
            club.Rooms.Select(r => new RoomDto(r.Id, r.Name ?? "", r.ClubId, r.IsActive ?? true)).ToList(),
            club.Masters.Select(m => new MasterDto(
                m.Id,
                m.Name ?? "",
                m.ClubId,
                m.UserId,
                m.User?.DisplayName,
                m.User?.Mobile,
                m.RatePerGame,
                m.Photo,
                m.Bio
            )).ToList()
        );
    }

    public async Task<ClubDto> CreateClubAsync(CreateClubDto dto)
    {
        var club = new Club
        {
            Name = PersianTextNormalizer.Normalize(dto.Name),
            Address = dto.Address,
            Phone = dto.Phone,
            City = dto.City,
            Description = dto.Description
        };
        _context.Clubs.Add(club);
        await _context.SaveChangesAsync();
        return new ClubDto(club.Id, club.Name ?? "", club.Address, club.Phone, club.City, club.Description, club.Logo);
    }

    public async Task<ClubDto?> UpdateClubAsync(int clubId, UpdateClubDto dto)
    {
        var club = await _context.Clubs.FindAsync(clubId);
        if (club is null) return null;

        if (dto.Name != null) club.Name = PersianTextNormalizer.Normalize(dto.Name);
        if (dto.Address != null) club.Address = dto.Address;
        if (dto.Phone != null) club.Phone = dto.Phone;
        if (dto.City != null) club.City = dto.City;
        if (dto.Description != null) club.Description = dto.Description;

        await _context.SaveChangesAsync();
        return new ClubDto(club.Id, club.Name ?? "", club.Address, club.Phone, club.City, club.Description, club.Logo);
    }

    public async Task<bool> DeleteClubAsync(int clubId)
    {
        var club = await _context.Clubs
            .Include(c => c.Rooms)
            .Include(c => c.Masters)
            .Include(c => c.Events)
                .ThenInclude(e => e.Clubplays)
            .FirstOrDefaultAsync(c => c.Id == clubId);

        if (club is null) return false;

        if (club.Rooms.Any())
            throw new InvalidOperationException("این باشگاه دارای اتاق است و قابل حذف نیست");

        if (club.Masters.Any())
            throw new InvalidOperationException("این باشگاه دارای گرداننده است و قابل حذف نیست");

        if (club.Events.Any(e => e.Clubplays.Any()))
            throw new InvalidOperationException("این باشگاه دارای بازی است و قابل حذف نیست");

        if (!string.IsNullOrEmpty(club.Logo))
        {
            var logoPath = Path.Combine(_env.WebRootPath, club.Logo.TrimStart('/'));
            if (File.Exists(logoPath))
                File.Delete(logoPath);
        }

        _context.Clubs.Remove(club);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoomDto> CreateRoomAsync(int clubId, CreateRoomDto dto)
    {
        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == clubId);
        if (!clubExists)
            throw new KeyNotFoundException("باشگاه یافت نشد");

        var room = new Room { Name = dto.Name, ClubId = clubId, IsActive = dto.IsActive };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return new RoomDto(room.Id, room.Name ?? "", room.ClubId, room.IsActive ?? true);
    }

    public async Task<RoomDto?> UpdateRoomAsync(int roomId, UpdateRoomDto dto)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room is null) return null;

        if (dto.Name != null) room.Name = dto.Name;
        if (dto.IsActive.HasValue) room.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return new RoomDto(room.Id, room.Name ?? "", room.ClubId, room.IsActive ?? true);
    }

    public async Task<bool> DeleteRoomAsync(int roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Clubplays)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room is null) return false;

        if (room.Clubplays.Any())
            throw new InvalidOperationException("این اتاق دارای بازی است و قابل حذف نیست");

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MasterDto> CreateMasterAsync(int clubId, CreateMasterDto dto)
    {
        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == clubId);
        if (!clubExists)
            throw new KeyNotFoundException("باشگاه یافت نشد");

        if (dto.ExistingUserId.HasValue && dto.NewUser != null)
            throw new InvalidOperationException("تنها یکی از ExistingUserId یا NewUser می‌تواند مقدار داشته باشد");

        if (dto.NewUser != null)
        {
            var mobileTaken = await _context.Users.AnyAsync(u => u.Mobile == dto.NewUser.Mobile);
            if (mobileTaken)
                throw new InvalidOperationException("این شماره موبایل قبلاً ثبت شده است");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        int? userId = dto.ExistingUserId;

        if (dto.NewUser != null)
        {
            var newUser = new User
            {
                Username = dto.NewUser.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewUser.Password),
                Mobile = dto.NewUser.Mobile,
                DisplayName = dto.NewUser.DisplayName,
                Role = "master",
                IsActive = true
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            userId = newUser.Id;
        }

        if (userId.HasValue)
        {
            var user = await _context.Users.FindAsync(userId.Value);
            if (user is null)
                throw new InvalidOperationException("کاربر یافت نشد");

            if (user.Role == "admin")
                throw new InvalidOperationException("این کاربر ادمین است و نمی‌تواند گرداننده شود");

            var duplicateUser = await _context.Masters
                .AnyAsync(m => m.UserId == userId.Value && m.ClubId == clubId);
            if (duplicateUser)
                throw new InvalidOperationException("این کاربر قبلاً به عنوان گرداننده در این باشگاه ثبت شده است");

            await UpsertClubUserAsync(userId.Value, clubId, "master");
            if (user.Role != "master")
            {
                user.Role = "master";
            }
        }

        var master = new Master
        {
            Name = PersianTextNormalizer.Normalize(dto.Name),
            ClubId = clubId,
            UserId = userId,
            RatePerGame = dto.RatePerGame
        };
        _context.Masters.Add(master);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        var resultUser = userId.HasValue
            ? await _context.Users.FindAsync(userId.Value)
            : null;

        return new MasterDto(
            master.Id, master.Name ?? "", master.ClubId,
            master.UserId, resultUser?.DisplayName, resultUser?.Mobile,
            master.RatePerGame, master.Photo, master.Bio
        );
    }

    public async Task<MasterDto?> UpdateMasterAsync(int masterId, UpdateMasterDto dto)
    {
        var master = await _context.Masters
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == masterId);

        if (master is null) return null;

        if (dto.Name != null) master.Name = PersianTextNormalizer.Normalize(dto.Name);
        if (dto.RatePerGame.HasValue) master.RatePerGame = dto.RatePerGame;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        if (dto.UnlinkUser)
        {
            var previousUserId = master.UserId;
            master.UserId = null;

            if (previousUserId.HasValue)
            {
                var clubUser = await _context.Clubusers
                    .FirstOrDefaultAsync(cu => cu.UserId == previousUserId.Value && cu.ClubId == master.ClubId);
                if (clubUser is not null)
                {
                    _context.Clubusers.Remove(clubUser);
                }
            }

            // On unlink, user.role is intentionally left untouched. The user may
            // still need the "master" role for other club associations or future
            // re-linking — we only clean up the per-club clubuser row.
        }
        else if (dto.ExistingUserId.HasValue && dto.ExistingUserId != master.UserId)
        {
            var user = await _context.Users.FindAsync(dto.ExistingUserId.Value);
            if (user is null)
                throw new InvalidOperationException("کاربر یافت نشد");

            if (user.Role == "admin")
                throw new InvalidOperationException("این کاربر ادمین است و نمی‌تواند گرداننده شود");

            var duplicateUser = await _context.Masters
                .AnyAsync(m => m.UserId == dto.ExistingUserId.Value && m.ClubId == master.ClubId && m.Id != masterId);
            if (duplicateUser)
                throw new InvalidOperationException("این کاربر قبلاً به عنوان گرداننده در این باشگاه ثبت شده است");

            master.UserId = dto.ExistingUserId;

            await UpsertClubUserAsync(dto.ExistingUserId.Value, master.ClubId, "master");
            if (user.Role != "master")
            {
                user.Role = "master";
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var resultUser = master.UserId.HasValue
            ? await _context.Users.FindAsync(master.UserId.Value)
            : null;

        return new MasterDto(
            master.Id, master.Name ?? "", master.ClubId,
            master.UserId, resultUser?.DisplayName, resultUser?.Mobile,
            master.RatePerGame, master.Photo, master.Bio
        );
    }

    public async Task<bool> DeleteMasterAsync(int masterId)
    {
        var master = await _context.Masters
            .Include(m => m.Clubplays)
            .FirstOrDefaultAsync(m => m.Id == masterId);

        if (master is null) return false;

        if (master.Clubplays.Any())
            throw new InvalidOperationException("این گرداننده دارای بازی است و قابل حذف نیست");

        if (!string.IsNullOrEmpty(master.Photo))
        {
            var photoPath = Path.Combine(_env.WebRootPath, master.Photo.TrimStart('/'));
            if (File.Exists(photoPath))
                File.Delete(photoPath);
        }

        _context.Masters.Remove(master);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> SaveLogoAsync(int clubId, IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل مجاز نیست. فقط jpg, jpeg, png, webp");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("حجم فایل باید کمتر از ۲ مگابایت باشد");

        var club = await _context.Clubs.FindAsync(clubId);
        if (club is null) return null;

        if (!string.IsNullOrEmpty(club.Logo))
        {
            var oldPath = Path.Combine(_env.WebRootPath, club.Logo.TrimStart('/'));
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "clubs");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/clubs/{fileName}";
        club.Logo = relativePath;
        await _context.SaveChangesAsync();

        return relativePath;
    }

    public async Task<string?> SaveMasterPhotoAsync(int masterId, IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل مجاز نیست. فقط jpg, jpeg, png, webp");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("حجم فایل باید کمتر از ۲ مگابایت باشد");

        var master = await _context.Masters.FindAsync(masterId);
        if (master is null) return null;

        if (!string.IsNullOrEmpty(master.Photo))
        {
            var oldPath = Path.Combine(_env.WebRootPath, master.Photo.TrimStart('/'));
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "masters");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/masters/{fileName}";
        master.Photo = relativePath;
        await _context.SaveChangesAsync();

        return relativePath;
    }

    private async Task UpsertClubUserAsync(int userId, int clubId, string role)
    {
        var clubUser = await _context.Clubusers
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ClubId == clubId);

        if (clubUser is null)
        {
            clubUser = new Clubuser
            {
                UserId = userId,
                ClubId = clubId,
                ClubuserRole = role
            };
            _context.Clubusers.Add(clubUser);
        }
        else
        {
            clubUser.ClubuserRole = role;
        }
    }
}
