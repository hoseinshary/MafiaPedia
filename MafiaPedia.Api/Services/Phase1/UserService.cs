using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.IServices.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase1;

public class UserService : IUserService
{
    private static readonly string[] ValidRoles = { "admin", "user", "master", "cafe_owner" };

    private readonly MafiaDbContext _context;

    public UserService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<UserListResponseDto> GetUsersAsync(int page, int pageSize, string? search)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = PersianTextNormalizer.Normalize(search.Trim());
            query = query.Where(u =>
                (u.Username != null && u.Username.Contains(term)) ||
                (u.DisplayName != null && u.DisplayName.Contains(term)) ||
                (u.Mobile != null && u.Mobile.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Mobile = u.Mobile,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return new UserListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<UserDetailDto?> GetUserDetailAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return null;

        return new UserDetailDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Mobile = user.Mobile,
            Role = user.Role,
            IsActive = user.IsActive,
            MobileVerified = user.MobileVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<UserDetailDto> CreateUserAsync(CreateUserDto dto)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            throw new InvalidOperationException("این نام کاربری قبلاً ثبت شده است");

        if (!ValidRoles.Contains(dto.Role))
            throw new InvalidOperationException($"نقش نامعتبر است. نقش‌های مجاز: {string.Join("، ", ValidRoles)}");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName = PersianTextNormalizer.Normalize(dto.DisplayName),
            Mobile = dto.Mobile ?? string.Empty,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDetailDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Mobile = user.Mobile,
            Role = user.Role,
            IsActive = user.IsActive,
            MobileVerified = user.MobileVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return false;

        if (dto.DisplayName != null)
            user.DisplayName = PersianTextNormalizer.Normalize(dto.DisplayName);

        if (dto.Mobile != null)
            user.Mobile = dto.Mobile;

        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive;

        if (dto.Password != null)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        if (dto.Role != null)
        {
            if (!ValidRoles.Contains(dto.Role))
                throw new ArgumentException($"نقش نامعتبر است. نقش‌های مجاز: {string.Join("، ", ValidRoles)}");

            user.Role = dto.Role;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Comments)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return (false, "کاربر یافت نشد");

        if (user.Comments.Any())
            return (false, "این کاربر دارای کامنت است و قابل حذف نیست");

        if (user.RefreshTokens.Any())
            _context.RefreshTokens.RemoveRange(user.RefreshTokens);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
