using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public class RankingService : IRankingService
{
    private readonly MafiaDbContext _context;

    public RankingService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RankingDto>> GetOverallRankingsAsync(int? clubId = null)
    {
        const int citizenSideId = 2;
        const int mafiaSideId = 1;

        IQueryable<Entities.Playplayer> query = _context.Playplayers;

        if (clubId.HasValue)
        {
            query = query.Where(pp => pp.Play.Event != null && pp.Play.Event.ClubId == clubId.Value);
        }

        var rankings = await query
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name })
            .Select(g => new
            {
                g.Key.PlayerId,
                g.Key.Name,
                TotalGames = g.Count(),
                OverallWins = g.Count(pp => pp.Play.WinnersideId == pp.Role.SideId),
                CitizenGames = g.Count(pp => pp.Role.SideId == citizenSideId),
                CitizenWins = g.Count(pp => pp.Role.SideId == citizenSideId && pp.Play.WinnersideId == citizenSideId),
                MafiaGames = g.Count(pp => pp.Role.SideId == mafiaSideId),
                MafiaWins = g.Count(pp => pp.Role.SideId == mafiaSideId && pp.Play.WinnersideId == mafiaSideId)
            })
            .Where(x => x.TotalGames >= 10)
            .OrderByDescending(x => x.OverallWins * 1.0 / x.TotalGames)
            .ToListAsync();

        return rankings.Select(r => new RankingDto
        {
            PlayerId = r.PlayerId,
            PlayerName = r.Name,
            TotalGames = r.TotalGames,
            OverallWinRate = Math.Round(r.OverallWins * 100.0 / r.TotalGames, 2),
            CitizenWinRate = r.CitizenGames > 0
                ? Math.Round(r.CitizenWins * 100.0 / r.CitizenGames, 2)
                : 0,
            MafiaWinRate = r.MafiaGames > 0
                ? Math.Round(r.MafiaWins * 100.0 / r.MafiaGames, 2)
                : 0
        });
    }

    public async Task<IEnumerable<SideRankingDto>> GetSideRankingsAsync(SideRankingFilterDto filter)
    {
        IQueryable<Entities.Playplayer> query = _context.Playplayers
            .Where(pp => pp.Role.SideId == filter.SideId);

        if (filter.ClubId.HasValue)
        {
            query = query.Where(pp => pp.Play.Event != null && pp.Play.Event.ClubId == filter.ClubId.Value);
        }

        if (filter.EventId.HasValue)
        {
            query = query.Where(pp => pp.Play.EventId == filter.EventId.Value);
        }

        if (filter.ScenarioId.HasValue)
        {
            query = query.Where(pp => pp.Play.SenarioId == filter.ScenarioId.Value);
        }

        var minGames = filter.MinimumGames;
        var sideId = filter.SideId;

        var rankings = await query
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name })
            .Select(g => new
            {
                g.Key.PlayerId,
                g.Key.Name,
                Games = g.Count(),
                Wins = g.Count(pp => pp.Play.WinnersideId == sideId)
            })
            .Where(x => x.Games >= minGames)
            .OrderByDescending(x => x.Wins * 1.0 / x.Games)
            .ThenByDescending(x => x.Games)
            .ToListAsync();

        return rankings.Select(r => new SideRankingDto
        {
            PlayerId = r.PlayerId,
            PlayerName = r.Name,
            Games = r.Games,
            Wins = r.Wins,
            WinRate = Math.Round(r.Wins * 100.0 / r.Games, 2)
        });
    }
}
