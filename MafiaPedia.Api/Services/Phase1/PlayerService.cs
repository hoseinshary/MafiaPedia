using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Services.Phase1;

public class PlayerService : IPlayerService
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PlayerService(MafiaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto)
    {
        string? picturePath = null;

        if (dto.Picture is not null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(dto.Picture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only jpg, jpeg, png, webp files are allowed.");

            if (dto.Picture.Length > 2 * 1024 * 1024)
                throw new InvalidOperationException("File size must be less than 2 MB.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "players");

            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Picture.CopyToAsync(stream);
            }

            picturePath = $"/uploads/players/{fileName}";
        }

        DateOnly? birthday = null;
        if (!string.IsNullOrWhiteSpace(dto.Birthday) && DateOnly.TryParse(dto.Birthday, out var parsed))
            birthday = parsed;

        var player = new Player
        {
            Name = PersianTextNormalizer.Normalize(dto.Name),
            Mobile = dto.Mobile,
            Code = dto.Code,
            Birthday = birthday,
            Desc = dto.Desc,
            Picture = picturePath
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return new PlayerDto
        {
            Id = player.Id,
            Name = player.Name!,
            Code = player.Code,
            Picture = player.Picture
        };
    }

    public async Task<PlayerProfileDto?> GetProfileAsync(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player is null) return null;

        const int citizenSideId = 2;
        const int mafiaSideId = 1;

        var stats = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId)
            .GroupBy(pp => pp.PlayerId)
            .Select(g => new
            {
                TotalGames = g.Count(),
                OverallWins = g.Count(pp => pp.Play.WinnersideId == pp.Role.SideId),
                CitizenGames = g.Count(pp => pp.Role.SideId == citizenSideId),
                CitizenWins = g.Count(pp => pp.Role.SideId == citizenSideId && pp.Play.WinnersideId == citizenSideId),
                MafiaGames = g.Count(pp => pp.Role.SideId == mafiaSideId),
                MafiaWins = g.Count(pp => pp.Role.SideId == mafiaSideId && pp.Play.WinnersideId == mafiaSideId)
            })
            .FirstOrDefaultAsync();

        var mostPlayedRoles = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId)
            .GroupBy(pp => new { pp.RoleId, pp.Role.Name })
            .Select(g => new PlayerRoleSummaryDto
            {
                RoleId = g.Key.RoleId,
                RoleName = g.Key.Name,
                Games = g.Count()
            })
            .OrderByDescending(r => r.Games)
            .Take(5)
            .ToListAsync();

        var bestRoles = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId)
            .GroupBy(pp => new { pp.RoleId, pp.Role.Name })
            .Select(g => new
            {
                RoleId = g.Key.RoleId,
                RoleName = g.Key.Name,
                Games = g.Count(),
                Wins = g.Count(pp => pp.Play.WinnersideId == pp.Role.SideId)
            })
            .Where(r => r.Games >= 3)
            .OrderByDescending(r => r.Wins * 1.0 / r.Games)
            .ThenByDescending(r => r.Games)
            .Take(5)
            .ToListAsync();

        var recentGames = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId)
            .OrderByDescending(pp => pp.Play.DateTime)
            .Take(20)
            .Select(pp => new PlayerRecentGameDto
            {
                PlayId = pp.PlayId,
                PlayTitle = pp.Play.Title,
                RoleName = pp.Role.Name,
                Result = pp.Play.WinnersideId == pp.Role.SideId ? "Win" : "Loss",
                Link = pp.Play.Link
            })
            .ToListAsync();

        var allPlayplayers = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId)
            .Include(pp => pp.Play)
            .Include(pp => pp.Role)
                .ThenInclude(r => r.Side)
            .OrderBy(pp => pp.Play.DateTime)
            .ToListAsync();

        var winStreak = 0;
        var orderedDesc = allPlayplayers.OrderByDescending(pp => pp.Play.DateTime).ToList();
        foreach (var pp in orderedDesc)
        {
            if (pp.Role?.SideId == pp.Play?.WinnersideId)
                winStreak++;
            else
                break;
        }

        var bestRun = 0;
        var currentRun = 0;
        foreach (var pp in allPlayplayers)
        {
            if (pp.Role?.SideId == pp.Play?.WinnersideId)
            {
                currentRun++;
                if (currentRun > bestRun) bestRun = currentRun;
            }
            else
            {
                currentRun = 0;
            }
        }

        var trend = new List<WinRateTrendDto>();
        for (var i = 9; i < allPlayplayers.Count; i++)
        {
            var windowStart = i - 9;
            var windowCount = i - windowStart + 1;
            var windowWins = 0;
            for (var j = windowStart; j <= i; j++)
            {
                if (allPlayplayers[j].Role?.SideId == allPlayplayers[j].Play?.WinnersideId)
                    windowWins++;
            }
            trend.Add(new WinRateTrendDto
            {
                GameIndex = i + 1,
                WinRate = Math.Round(windowWins * 100.0 / windowCount, 2)
            });
        }

        var mafiaPartner = await FindBestPartnerAsync(playerId, mafiaSideId);
        var citizenPartner = await FindBestPartnerAsync(playerId, citizenSideId);

        return new PlayerProfileDto
        {
            Id = player.Id,
            Name = player.Name,
            Picture = player.Picture,
            Birthday = player.Birthday,
            Statistics = new PlayerStatisticsDto
            {
                TotalGames = stats?.TotalGames ?? 0,
                OverallWinRate = stats is not null && stats.TotalGames > 0
                    ? Math.Round(stats.OverallWins * 100.0 / stats.TotalGames, 1)
                    : 0,
                CitizenGames = stats?.CitizenGames ?? 0,
                CitizenWinRate = stats is not null && stats.CitizenGames > 0
                    ? Math.Round(stats.CitizenWins * 100.0 / stats.CitizenGames, 1)
                    : 0,
                MafiaGames = stats?.MafiaGames ?? 0,
                MafiaWinRate = stats is not null && stats.MafiaGames > 0
                    ? Math.Round(stats.MafiaWins * 100.0 / stats.MafiaGames, 1)
                    : 0
            },
            MostPlayedRoles = mostPlayedRoles,
            BestRoles = bestRoles.Select(r => new PlayerRoleSummaryDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Games = r.Games,
                Wins = r.Wins,
                WinRate = Math.Round(r.Wins * 100.0 / r.Games, 2)
            }).ToList(),
            RecentGames = recentGames,
            WinStreak = winStreak,
            BestRun = bestRun,
            BestMafiaPartner = mafiaPartner,
            BestCitizenPartner = citizenPartner,
            WinRateTrend = trend
        };
    }

    private async Task<BestPartnerDto?> FindBestPartnerAsync(int playerId, int sideId)
    {
        var playIds = await _context.Playplayers
            .Where(pp => pp.PlayerId == playerId && pp.Role.SideId == sideId)
            .Select(pp => pp.PlayId)
            .ToListAsync();

        if (playIds.Count == 0) return null;

        var partner = await _context.Playplayers
            .Where(pp => playIds.Contains(pp.PlayId) && pp.PlayerId != playerId && pp.Role.SideId == sideId)
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name })
            .Select(g => new
            {
                g.Key.PlayerId,
                g.Key.Name,
                SharedGames = g.Count(),
                Wins = g.Count(pp => pp.Role.SideId == pp.Play.WinnersideId)
            })
            .Where(x => x.SharedGames >= 3)
            .OrderByDescending(x => x.Wins * 1.0 / x.SharedGames)
            .ThenByDescending(x => x.SharedGames)
            .FirstOrDefaultAsync();

        if (partner is null) return null;

        return new BestPartnerDto
        {
            PlayerId = partner.PlayerId,
            PlayerName = partner.Name ?? "",
            SharedGames = partner.SharedGames,
            WinRate = Math.Round(partner.Wins * 100.0 / partner.SharedGames, 2)
        };
    }

    public async Task<PlayerListResponseDto> GetPlayersAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = PersianTextNormalizer.Normalize(search.Trim());
            query = query.Where(p =>
                (p.Name != null && p.Name.Contains(term)) ||
                (p.Code != null && p.Code.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlayerListDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Picture = p.Picture,
                Mobile = p.Mobile,
                TotalGames = p.Playplayers.Count
            })
            .ToListAsync();

        return new PlayerListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<bool> UpdatePlayerAsync(int playerId, UpdatePlayerDto dto)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player is null) return false;

        if (dto.Name != null) player.Name = PersianTextNormalizer.Normalize(dto.Name);
        if (dto.Mobile != null) player.Mobile = dto.Mobile;
        if (dto.Code != null) player.Code = dto.Code;
        if (dto.Desc != null) player.Desc = dto.Desc;

        if (dto.Birthday != null)
        {
            if (DateOnly.TryParse(dto.Birthday, out var parsed))
                player.Birthday = parsed;
        }

        if (dto.Picture != null)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(dto.Picture.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new ArgumentException("فرمت فایل مجاز نیست");
            if (dto.Picture.Length > 3 * 1024 * 1024)
                throw new ArgumentException("حجم فایل بیش از ۲ مگابایت است");

            if (!string.IsNullOrEmpty(player.Picture))
            {
                var oldPath = Path.Combine(_env.WebRootPath, player.Picture.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var filename = $"{Guid.NewGuid()}{ext}";
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "players");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var path = Path.Combine(uploadsDir, filename);
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await dto.Picture.CopyToAsync(stream);
            }

            player.Picture = $"/uploads/players/{filename}";
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PlayerDetailDto?> GetPlayerDetailAsync(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player is null) return null;

        return new PlayerDetailDto
        {
            Id = player.Id,
            Name = player.Name,
            Mobile = player.Mobile,
            Code = player.Code,
            Birthday = player.Birthday?.ToString(),
            Desc = player.Desc,
            Picture = player.Picture
        };
    }

    public async Task<(bool Success, string? Error)> DeletePlayerAsync(int playerId)
    {
        var player = await _context.Players
            .Include(p => p.Playplayers)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player is null)
            return (false, "بازیکن یافت نشد");

        if (player.Playplayers.Any())
            return (false, $"این بازیکن در {player.Playplayers.Count} بازی شرکت داشته و قابل حذف نیست");

        if (!string.IsNullOrEmpty(player.Picture))
        {
            var picturePath = Path.Combine(_env.WebRootPath, player.Picture.TrimStart('/'));
            if (File.Exists(picturePath))
                File.Delete(picturePath);
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IEnumerable<PlayerSearchDto>> SearchPlayersAsync(string query, int limit = 10)
    {
        var normalizedQuery = PersianTextNormalizer.Normalize(query);
        var results = await _context.Players
            .AsNoTracking()
            .Where(p => p.Name != null && p.Name.Contains(normalizedQuery))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Picture,
                TotalGames = p.Playplayers.Count
            })
            .OrderByDescending(x => x.TotalGames)
            .ThenBy(x => x.Name)
            .Take(limit)
            .ToListAsync();

        return results.Select(x => new PlayerSearchDto
        {
            Id = x.Id,
            Name = x.Name,
            Picture = x.Picture,
            TotalGames = x.TotalGames
        });
    }
}
