using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public class PlayerService : IPlayerService
{
    private readonly MafiaDbContext _context;

    public PlayerService(MafiaDbContext context)
    {
        _context = context;
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
            RecentGames = recentGames
        };
    }
}
