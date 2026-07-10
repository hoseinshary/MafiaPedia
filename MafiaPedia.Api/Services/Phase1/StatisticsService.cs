using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Services.Phase1;

public class StatisticsService : IStatisticsService
{
    private readonly MafiaDbContext _context;

    public StatisticsService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<StatisticsDto> GetStatisticsAsync(StatisticsFilterDto filter)
    {
        IQueryable<Entities.Play> query = _context.Plays;

        if (filter.ClubId.HasValue)
            query = query.Where(p => p.Event != null && p.Event.ClubId == filter.ClubId.Value);

        if (filter.EventId.HasValue)
            query = query.Where(p => p.EventId == filter.EventId.Value);

        if (filter.ScenarioId.HasValue)
            query = query.Where(p => p.SenarioId == filter.ScenarioId.Value);

        var totalGames = await query.CountAsync();

        var totalPlayers = await query
            .SelectMany(p => p.Playplayers)
            .Select(pp => pp.PlayerId)
            .Distinct()
            .CountAsync();

        var byClub = await query
            .Where(p => p.Event != null && p.Event.Club != null)
            .GroupBy(p => new { Id = p.Event.Club!.Id, Name = p.Event.Club!.Name })
            .Select(g => new SideWinRateDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name ?? "",
                TotalGames = g.Count(),
                MafiaWinRate = g.Count(p => p.WinnersideId == 1) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 1) * 100.0 / g.Count(), 2) : 0,
                CitizenWinRate = g.Count(p => p.WinnersideId == 2) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 2) * 100.0 / g.Count(), 2) : 0
            })
            .ToListAsync();

        var byEvent = await query
            .Where(p => p.Event != null)
            .GroupBy(p => new { p.Event!.Id, p.Event!.Name })
            .Select(g => new SideWinRateDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name ?? "",
                TotalGames = g.Count(),
                MafiaWinRate = g.Count(p => p.WinnersideId == 1) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 1) * 100.0 / g.Count(), 2) : 0,
                CitizenWinRate = g.Count(p => p.WinnersideId == 2) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 2) * 100.0 / g.Count(), 2) : 0
            })
            .ToListAsync();

        var byScenario = await query
            .Where(p => p.Senario != null)
            .GroupBy(p => new { p.Senario!.Id, p.Senario!.Name })
            .Select(g => new SideWinRateDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name ?? "",
                TotalGames = g.Count(),
                MafiaWinRate = g.Count(p => p.WinnersideId == 1) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 1) * 100.0 / g.Count(), 2) : 0,
                CitizenWinRate = g.Count(p => p.WinnersideId == 2) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 2) * 100.0 / g.Count(), 2) : 0
            })
            .ToListAsync();

        var trend = await query
            .Where(p => p.DateTime != null)
            .GroupBy(p => new { p.DateTime!.Value.Year, p.DateTime!.Value.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyWinRateTrendDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalGames = g.Count(),
                MafiaWinRate = g.Count(p => p.WinnersideId == 1) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 1) * 100.0 / g.Count(), 2) : 0,
                CitizenWinRate = g.Count(p => p.WinnersideId == 2) > 0
                    ? Math.Round(g.Count(p => p.WinnersideId == 2) * 100.0 / g.Count(), 2) : 0
            })
            .ToListAsync();

        return new StatisticsDto
        {
            TotalGames = totalGames,
            TotalPlayers = totalPlayers,
            WinRateByClub = byClub,
            WinRateByEvent = byEvent,
            WinRateByScenario = byScenario,
            WinRateTrend = trend
        };
    }

    public async Task<StatisticsHomeDto> GetStatisticsHomeAsync()
    {
        // آمار کلی
        var totalGames = await _context.Plays.CountAsync();
        var totalPlayers = await _context.Players.CountAsync();
        var totalSenarios = await _context.Senarios.CountAsync();
        var totalEvents = await _context.Events.CountAsync();

        // دریافت آخرین ۵ بازی با اطلاعات مرتبط
        var last5Plays = await _context.Plays
            .Include(p => p.Senario)
            .Include(p => p.Event)
            .ThenInclude(e => e.Club)
            .Include(p => p.Winnerside)
            .OrderByDescending(p => p.DateTime)
            .Take(5)
            .Select(p => new PlayListDto
            {
                Id = p.Id,
                Title = p.Title,
                DateTime = p.DateTime,
                Link = p.Link,
                SenarioName = p.Senario != null ? p.Senario.Name : "",
                EventName = p.Event != null ? p.Event.Name : "",
                ClubName = p.Event != null && p.Event.Club != null ? p.Event.Club.Name : "",
                WinnersideName = p.Winnerside != null ? p.Winnerside.Name : ""
            })
            .ToListAsync();

        const int minGames = 10;

        var allPlayersRanking = await _context.Playplayers
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name, pp.Player.Picture })
            .Select(g => new
            {
                PlayerId = g.Key.PlayerId,
                PlayerName = g.Key.Name,
                Picture = g.Key.Picture,
                TotalGames = g.Count(),
                OverallWins = g.Count(pp => pp.Play.WinnersideId == pp.Role.SideId),
                CitizenGames = g.Count(pp => pp.Role.SideId == 2),
                CitizenWins = g.Count(pp => pp.Role.SideId == 2 && pp.Play.WinnersideId == 2),
                MafiaGames = g.Count(pp => pp.Role.SideId == 1),
                MafiaWins = g.Count(pp => pp.Role.SideId == 1 && pp.Play.WinnersideId == 1)
            })
            .Where(x => x.TotalGames >= 20)
            .OrderByDescending(x => x.OverallWins * 1.0 / x.TotalGames)
            .ThenByDescending(x => x.TotalGames)
            .Take(3)
            .ToListAsync();

        var allTop3Player = allPlayersRanking.Select(r => new RankingDto
        {
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            Picture = r.Picture,
            TotalGames = r.TotalGames,
            OverallWinRate = Math.Round(r.OverallWins * 100.0 / r.TotalGames, 2),
            CitizenWinRate = r.CitizenGames > 0
                ? Math.Round(r.CitizenWins * 100.0 / r.CitizenGames, 2) : 0,
            MafiaWinRate = r.MafiaGames > 0
                ? Math.Round(r.MafiaWins * 100.0 / r.MafiaGames, 2) : 0
        }).ToList();

        var citizenRanking = await _context.Playplayers
            .Where(pp => pp.Role.SideId == 2)
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name, pp.Player.Picture })
            .Select(g => new
            {
                PlayerId = g.Key.PlayerId,
                PlayerName = g.Key.Name,
                Picture = g.Key.Picture,
                Games = g.Count(),
                Wins = g.Count(pp => pp.Play.WinnersideId == 2)
            })
            .Where(x => x.Games >= minGames)
            .OrderByDescending(x => x.Wins * 1.0 / x.Games)
            .ThenByDescending(x => x.Games)
            .Take(3)
            .ToListAsync();

        var citizenTop3Player = citizenRanking.Select(r => new SideRankingDto
        {
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            Picture = r.Picture,
            Games = r.Games,
            Wins = r.Wins,
            WinRate = Math.Round(r.Wins * 100.0 / r.Games, 2)
        }).ToList();

        var mafiaRanking = await _context.Playplayers
            .Where(pp => pp.Role.SideId == 1)
            .GroupBy(pp => new { pp.PlayerId, pp.Player.Name, pp.Player.Picture })
            .Select(g => new
            {
                PlayerId = g.Key.PlayerId,
                PlayerName = g.Key.Name,
                Picture = g.Key.Picture,
                Games = g.Count(),
                Wins = g.Count(pp => pp.Play.WinnersideId == 1)
            })
            .Where(x => x.Games >= minGames)
            .OrderByDescending(x => x.Wins * 1.0 / x.Games)
            .ThenByDescending(x => x.Games)
            .Take(3)
            .ToListAsync();

        var mafiaTop3Player = mafiaRanking.Select(r => new SideRankingDto
        {
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            Picture = r.Picture,
            Games = r.Games,
            Wins = r.Wins,
            WinRate = Math.Round(r.Wins * 100.0 / r.Games, 2)
        }).ToList();

        var donclubTask = await GetClubStatAsync(1);
        var legendaryTask = await GetClubStatAsync(2);
        //await Task.WhenAll(donclubTask, legendaryTask);

        return new StatisticsHomeDto
        {
            TotalGames = totalGames,
            TotalPlayers = totalPlayers,
            TotalSenarios = totalSenarios,
            TotalEvents = totalEvents,
            Last5Plays = last5Plays,
            AllTop3Player = allTop3Player,
            CitizenTop3Player = citizenTop3Player,
            MafiaTop3Player = mafiaTop3Player,
            DonclubStat = donclubTask,
            LegendaryStat = legendaryTask
        };

    }

    private async Task<ClubStatDto> GetClubStatAsync(int clubId)
    {
        var clubName = await _context.Clubs
            .Where(c => c.Id == clubId)
            .Select(c => c.Name ?? "")
            .FirstOrDefaultAsync() ?? "";

        IQueryable<Entities.Play> playQuery = _context.Plays.Where(p => p.Event != null && p.Event.ClubId == clubId);
        
        

        var playStats = await playQuery
            .GroupBy(p => 1)
            .Select(g => new
            {
                PlayCount = g.Count(),
                MafiaWins = g.Count(p => p.WinnersideId == 1)
            })
            .FirstOrDefaultAsync();

        var playerCount = await playQuery
            .SelectMany(p => p.Playplayers)
            .Select(pp => pp.PlayerId)
            .Distinct()
            .CountAsync();

        var playCount = playStats?.PlayCount ?? 0;
        var mafiaWins = playStats?.MafiaWins ?? 0;

        return new ClubStatDto
        {
            ClubId = clubId,
            ClubName = clubName,
            PlayCount = playCount,
            PlayerCount = playerCount,
            MafiaWinRate = playCount > 0
                ? Math.Round(mafiaWins * 100.0 / playCount, 2)
                : 0
        };
    }
    
}