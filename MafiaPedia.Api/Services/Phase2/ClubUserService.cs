using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Club;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubUserService : IClubUserService
{
    private readonly MafiaDbContext _context;

    public ClubUserService(MafiaDbContext context)
    {
        _context = context;
    }

    private static readonly HashSet<string> AllowedRoles = new() { "owner", "supervisor", "cashier" };

    public async Task<List<ClubUserDto>> GetMembersAsync(int clubId)
    {
        return await _context.Clubusers
            .Where(cu => cu.ClubId == clubId)
            .Include(cu => cu.User)
            .Select(cu => new ClubUserDto(
                cu.Id,
                cu.UserId,
                cu.User.DisplayName,
                cu.User.Mobile,
                cu.ClubuserRole,
                cu.ClubId,
                _context.Masters
                    .Where(m => m.UserId == cu.UserId && m.ClubId == clubId)
                    .Select(m => (int?)m.Id)
                    .FirstOrDefault(),
                _context.Masters
                    .Where(m => m.UserId == cu.UserId && m.ClubId == clubId)
                    .Select(m => m.Name)
                    .FirstOrDefault()
            ))
            .ToListAsync();
    }

    public async Task<ClubUserDto> CreateMemberAsync(int clubId, CreateClubUserDto dto, bool callerIsAdmin)
    {
        if (!AllowedRoles.Contains(dto.ClubuserRole))
            throw new InvalidOperationException("این سرویس فقط برای owner/supervisor/cashier است");

        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == clubId);
        if (!clubExists)
            throw new KeyNotFoundException("باشگاه یافت نشد");

        if (dto.ClubuserRole == "owner" && !callerIsAdmin)
            throw new UnauthorizedAccessException();

        if (dto.ExistingUserId.HasValue && dto.NewUser != null)
            throw new InvalidOperationException("تنها یکی از ExistingUserId یا NewUser می‌تواند مقدار داشته باشد");

        if (!dto.ExistingUserId.HasValue && dto.NewUser == null)
            throw new InvalidOperationException("تنها یکی از ExistingUserId یا NewUser می‌تواند مقدار داشته باشد");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        int userId;

        if (dto.NewUser != null)
        {
            var mobileTaken = await _context.Users.AnyAsync(u => u.Mobile == dto.NewUser.Mobile);
            if (mobileTaken)
                throw new InvalidOperationException("این شماره موبایل قبلاً ثبت شده است");

            var newUser = new User
            {
                Username = dto.NewUser.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewUser.Password),
                Mobile = dto.NewUser.Mobile,
                DisplayName = dto.NewUser.DisplayName,
                Role = "club",
                IsActive = true
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            userId = newUser.Id;
        }
        else
        {
            userId = dto.ExistingUserId!.Value;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            throw new InvalidOperationException("کاربر یافت نشد");

        if (user.Role == "admin")
            throw new InvalidOperationException("این کاربر ادمین است و نمی‌تواند نقش کلاب بگیرد");

        var existingClubUser = await _context.Clubusers
            .AnyAsync(cu => cu.UserId == userId && cu.ClubId == clubId);
        if (existingClubUser)
            throw new InvalidOperationException("این کاربر قبلاً در این باشگاه نقشی دارد — از ویرایش نقش استفاده کنید");

        var clubUser = new Clubuser
        {
            UserId = userId,
            ClubId = clubId,
            ClubuserRole = dto.ClubuserRole
        };
        _context.Clubusers.Add(clubUser);

        if (user.Role != "club")
        {
            user.Role = "club";
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ClubUserDto(
            clubUser.Id,
            clubUser.UserId,
            user.DisplayName,
            user.Mobile,
            clubUser.ClubuserRole,
            clubUser.ClubId,
            null,
            null
        );
    }

    public async Task<ClubUserDto?> UpdateMemberRoleAsync(int clubId, int clubUserId, UpdateClubUserRoleDto dto, bool callerIsAdmin)
    {
        var clubUser = await _context.Clubusers
            .Include(cu => cu.User)
            .FirstOrDefaultAsync(cu => cu.Id == clubUserId && cu.ClubId == clubId);

        if (clubUser is null) return null;

        if (dto.ClubuserRole == "master")
        {
            var hasMaster = await _context.Masters
                .AnyAsync(m => m.UserId == clubUser.UserId && m.ClubId == clubId);
            if (!hasMaster)
                throw new InvalidOperationException("این کاربر پروفایل گرداننده ندارد");
        }
        else if (!AllowedRoles.Contains(dto.ClubuserRole))
        {
            throw new InvalidOperationException("این سرویس فقط برای owner/supervisor/cashier است");
        }

        if (dto.ClubuserRole == "owner" && !callerIsAdmin)
            throw new UnauthorizedAccessException();

        clubUser.ClubuserRole = dto.ClubuserRole;
        await _context.SaveChangesAsync();

        var masterInfo = await _context.Masters
            .Where(m => m.UserId == clubUser.UserId && m.ClubId == clubId)
            .Select(m => new { m.Id, m.Name })
            .FirstOrDefaultAsync();

        return new ClubUserDto(
            clubUser.Id,
            clubUser.UserId,
            clubUser.User?.DisplayName,
            clubUser.User?.Mobile,
            clubUser.ClubuserRole,
            clubUser.ClubId,
            masterInfo?.Id,
            masterInfo?.Name
        );
    }

    public async Task<bool> DeleteMemberAsync(int clubId, int clubUserId)
    {
        var clubUser = await _context.Clubusers
            .FirstOrDefaultAsync(cu => cu.Id == clubUserId && cu.ClubId == clubId);

        if (clubUser is null) return false;

        _context.Clubusers.Remove(clubUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Clubuser?> GetClubUserAsync(int userId, int clubId)
    {
        return await _context.Clubusers
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ClubId == clubId);
    }

    public async Task<List<ClubUserContextDto>> GetMyClubsAsync(int userId)
    {
        return await _context.Clubusers
            .Where(cu => cu.UserId == userId)
            .Include(cu => cu.Club)
            .Select(cu => new ClubUserContextDto(
                cu.ClubId,
                cu.Club.Name ?? "",
                cu.ClubuserRole
            ))
            .ToListAsync();
    }
}
