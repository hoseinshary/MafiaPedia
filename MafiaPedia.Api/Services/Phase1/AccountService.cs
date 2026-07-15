using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Services.Phase1;

public class AccountService : IAccountService
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AccountService(MafiaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<AccountDto> GetMyAccountAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Player)
            .Include(u => u.Clubplayer)
            .Include(u => u.Master)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new KeyNotFoundException("کاربر یافت نشد");

        return MapToDto(user);
    }

    public async Task<AccountDto> UpdateAccountAsync(int userId, UpdateAccountDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Player)
            .Include(u => u.Clubplayer)
            .Include(u => u.Master)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new KeyNotFoundException("کاربر یافت نشد");

        if (dto.Username is not null)
        {
            if (dto.Username.Length > 20)
                throw new ArgumentException("نام کاربری حداکثر ۲۰ کاراکتر می‌تواند باشد");

            var normalized = dto.Username.Trim();
            if (!string.Equals(normalized, user.Username, StringComparison.OrdinalIgnoreCase))
            {
                var taken = await _context.Users.AnyAsync(u => u.Username == normalized && u.Id != userId);
                if (taken)
                    throw new InvalidOperationException("این نام کاربری قبلاً استفاده شده است");
                user.Username = normalized;
            }
        }

        if (dto.DisplayName is not null)
        {
            if (dto.DisplayName.Length > 50)
                throw new ArgumentException("نام نمایشی حداکثر ۵۰ کاراکتر می‌تواند باشد");
            user.DisplayName = dto.DisplayName.Trim();
        }

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("کاربر یافت نشد");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new ArgumentException("رمز عبور فعلی اشتباه است");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<string> UpdateLinkedPictureAsync(int userId, string target, IFormFile file)
    {
        var normalizedTarget = target.ToLowerInvariant();
        if (normalizedTarget is not "player" and not "clubplayer" and not "master")
            throw new ArgumentException("هدف باید player، clubplayer یا master باشد");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("فقط فایل‌های jpg، jpeg، png و webp مجاز هستند");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("حجم فایل باید کمتر از ۵ مگابایت باشد");

        if (normalizedTarget == "player")
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player is null)
                throw new InvalidOperationException("شما به یک بازیکن لینک نیستید");

            var newPath = SaveFile(file, "players");
            DeleteOldFile(player.Picture);
            player.Picture = newPath;
            await _context.SaveChangesAsync();
            return newPath;
        }
        else if (normalizedTarget == "clubplayer")
        {
            var clubplayer = await _context.Clubplayers.FirstOrDefaultAsync(cp => cp.UserId == userId);
            if (clubplayer is null)
                throw new InvalidOperationException("شما به یک مشتری کافه لینک نیستید");

            var newPath = SaveFile(file, "clubplayers");
            DeleteOldFile(clubplayer.Picture);
            clubplayer.Picture = newPath;
            await _context.SaveChangesAsync();
            return newPath;
        }
        else // master
        {
            var master = await _context.Masters.FirstOrDefaultAsync(m => m.UserId == userId);
            if (master is null)
                throw new InvalidOperationException("شما به یک گرداننده لینک نیستید");

            var newPath = SaveFile(file, "masters");
            DeleteOldFile(master.Photo);
            master.Photo = newPath;
            await _context.SaveChangesAsync();
            return newPath;
        }
    }

    private string SaveFile(IFormFile file, string subfolder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subfolder);

        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return $"/uploads/{subfolder}/{fileName}";
    }

    private static void DeleteOldFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static AccountDto MapToDto(User user)
    {
        return new AccountDto
        {
            Id = user.Id,
            Username = user.Username ?? "",
            DisplayName = user.DisplayName ?? "",
            Mobile = user.Mobile ?? "",
            Role = user.Role ?? "",
            Player = user.Player is not null
                ? new LinkedPlayerDto { Id = user.Player.Id, Name = user.Player.Name ?? "", Picture = user.Player.Picture }
                : null,
            Clubplayer = user.Clubplayer is not null
                ? new LinkedClubplayerDto { Id = user.Clubplayer.Id, Name = user.Clubplayer.Name ?? "", Picture = user.Clubplayer.Picture }
                : null,
            Master = user.Master is not null
                ? new LinkedMasterDto { Id = user.Master.Id, Name = user.Master.Name ?? "", Photo = user.Master.Photo }
                : null,
        };
    }
}
