using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.ClubPlayer;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase2;

public class ClubPlayerService : IClubPlayerService
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ClubPlayerService(MafiaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<(List<ClubPlayerDto> Items, int Total)> GetClubPlayersAsync(int clubId, int page, int pageSize, string? search)
    {
        var query = _context.ClubClubplayers
            .Where(cc => cc.ClubId == clubId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = PersianTextNormalizer.Normalize(search.Trim());
            query = query.Where(cc =>
                (cc.Clubplayer.Name != null && cc.Clubplayer.Name.Contains(term)) ||
                cc.Clubplayer.Mobile.Contains(term));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(cc => cc.Clubplayer.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cc => new ClubPlayerDto(
                cc.Clubplayer.Id,
                cc.Clubplayer.Name ?? "",
                cc.Clubplayer.Mobile,
                cc.Clubplayer.Birthday != null ? cc.Clubplayer.Birthday.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                cc.Clubplayer.Code,
                cc.Clubplayer.Picture,
                cc.Clubplayer.Desc,
                cc.JoinedAt
            ))
            .ToListAsync();

        return (items, total);
    }

    public async Task<ClubPlayerDto?> GetClubPlayerDetailAsync(int clubId, int customerId)
    {
        var membership = await _context.ClubClubplayers
            .FirstOrDefaultAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == customerId);

        if (membership is null) return null;

        var player = await _context.Clubplayers.FindAsync(customerId);
        if (player is null) return null;

        return ToDto(player, membership.JoinedAt);
    }

    public async Task<ClubPlayerDto?> SearchByMobileAsync(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile) || mobile.Length != 11)
            return null;

        var player = await _context.Clubplayers
            .FirstOrDefaultAsync(p => p.Mobile == mobile);

        return player is null ? null : ToDto(player);
    }

    public async Task<ClubPlayerJoinResultDto> CreateOrJoinAsync(int clubId, CreateOrJoinClubPlayerDto dto, string? picturePath)
    {
        var existing = await _context.Clubplayers
            .FirstOrDefaultAsync(p => p.Mobile == dto.Mobile);

        var now = DateTime.UtcNow;

        if (existing is null)
        {
            DateOnly? birthday = dto.Birthday.HasValue ? (DateOnly?)DateOnly.FromDateTime(dto.Birthday.Value) : null;

            var player = new Clubplayer
            {
                Name = PersianTextNormalizer.Normalize(dto.Name),
                Mobile = dto.Mobile,
                Birthday = birthday,
                Code = dto.Code,
                Desc = dto.Desc,
                Picture = picturePath
            };
            _context.Clubplayers.Add(player);
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO club_clubplayer (clubId, clubplayerId, JoinedAt) VALUES ({0}, {1}, {2})",
                clubId, player.Id, now);

            return new ClubPlayerJoinResultDto(ToDto(player, now), false);
        }

        var alreadyMember = await _context.ClubClubplayers
            .AnyAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == existing.Id);

        if (alreadyMember)
            throw new InvalidOperationException("این مشتری قبلاً عضو این کافه است");

        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO club_clubplayer (clubId, clubplayerId, JoinedAt) VALUES ({0}, {1}, {2})",
            clubId, existing.Id, now);

        return new ClubPlayerJoinResultDto(ToDto(existing, now), true);
    }

    public async Task<ClubPlayerDto?> UpdateClubPlayerAsync(int customerId, UpdateClubPlayerDto dto, string? newPicturePath)
    {
        var player = await _context.Clubplayers.FindAsync(customerId);
        if (player is null) return null;

        if (dto.Name != null) player.Name = PersianTextNormalizer.Normalize(dto.Name);
        if (dto.Birthday.HasValue) player.Birthday = DateOnly.FromDateTime(dto.Birthday.Value);
        if (dto.Code != null) player.Code = dto.Code;
        if (dto.Desc != null) player.Desc = dto.Desc;

        if (newPicturePath != null)
        {
            if (!string.IsNullOrEmpty(player.Picture))
            {
                var oldPath = Path.Combine(_env.WebRootPath, player.Picture.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }
            player.Picture = newPicturePath;
        }

        await _context.SaveChangesAsync();
        return ToDto(player);
    }

    public async Task<CustomerSearchResultDto> SearchAllAsync(int clubId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new CustomerSearchResultDto(new List<ClubPlayerDto>(), new List<ClubPlayerDto>());

        var term = PersianTextNormalizer.Normalize(query.Trim());

        var inClub = await _context.ClubClubplayers
            .Where(cc => cc.ClubId == clubId)
            .Where(cc =>
                (cc.Clubplayer.Name != null && cc.Clubplayer.Name.Contains(term)) ||
                cc.Clubplayer.Mobile.Contains(term))
            .OrderBy(cc => cc.Clubplayer.Name)
            .Select(cc => new ClubPlayerDto(
                cc.Clubplayer.Id, cc.Clubplayer.Name ?? "", cc.Clubplayer.Mobile,
                cc.Clubplayer.Birthday != null ? cc.Clubplayer.Birthday.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                cc.Clubplayer.Code, cc.Clubplayer.Picture, cc.Clubplayer.Desc,
                cc.JoinedAt))
            .ToListAsync();

        var globalOthers = await _context.Clubplayers
            .Where(p => 
                ((p.Name != null && p.Name.Contains(term)) || p.Mobile.Contains(term)))
            .OrderBy(p => p.Name)
            .Select(p => new ClubPlayerDto(
                p.Id, p.Name ?? "", p.Mobile,
                p.Birthday != null ? p.Birthday.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                p.Code, p.Picture, p.Desc,
                null))
            .ToListAsync();

        return new CustomerSearchResultDto(inClub, globalOthers);
    }

    public async Task<bool> RemoveFromClubAsync(int clubId, int customerId)
    {
        var membership = await _context.ClubClubplayers
            .FirstOrDefaultAsync(cc => cc.ClubId == clubId && cc.ClubplayerId == customerId);

        if (membership is null) return false;

        var hasGames = await _context.Clubplayplayers
            .AnyAsync(cpp => cpp.PlayerId == customerId && cpp.Play.Room.ClubId == clubId);

        if (hasGames)
            throw new InvalidOperationException("این مشتری در بازی‌های این کافه ثبت شده است و قابل حذف نیست");

        _context.ClubClubplayers.Remove(membership);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ClubPlayerDto ToDto(Clubplayer p, DateTime? joinedAt = null)
    {
        return new ClubPlayerDto(
            p.Id,
            p.Name ?? "",
            p.Mobile,
            p.Birthday?.ToDateTime(TimeOnly.MinValue),
            p.Code,
            p.Picture,
            p.Desc,
            joinedAt
        );
    }
}
