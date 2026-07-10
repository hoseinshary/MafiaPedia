using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Services.Phase1;

public class PlayReadService : IPlayReadService
{
    private readonly MafiaDbContext _context;

    public PlayReadService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<HeadToHeadDto?> GetHeadToHeadAsync(int player1Id, int player2Id)
    {
        var player1 = await _context.Players.FindAsync(player1Id);
        var player2 = await _context.Players.FindAsync(player2Id);

        if (player1 is null || player2 is null) return null;

        var player1PlayIds = await _context.Playplayers
            .Where(pp => pp.PlayerId == player1Id)
            .Select(pp => pp.PlayId)
            .ToListAsync();

        var sharedPlayIds = await _context.Playplayers
            .Where(pp => pp.PlayerId == player2Id && player1PlayIds.Contains(pp.PlayId))
            .Select(pp => pp.PlayId)
            .ToListAsync();

        var sharedPlays = await _context.Plays
            .Include(p => p.Winnerside)
            .Include(p => p.Playplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .Where(p => sharedPlayIds.Contains(p.Id))
            .OrderByDescending(p => p.DateTime)
            .ToListAsync();

        var totalShared = sharedPlays.Count;

        int oppositeCount = 0, p1WinsOpposite = 0, p2WinsOpposite = 0, draws = 0;
        int sameMafiaCount = 0, sameMafiaWins = 0, sameMafiaLosses = 0;
        int sameCitizenCount = 0, sameCitizenWins = 0, sameCitizenLosses = 0;
        int totalP1Wins = 0, totalP2Wins = 0;

        var sharedPlayDtos = new List<SharedPlayDto>();

        foreach (var play in sharedPlays)
        {
            var pp1 = play.Playplayers.FirstOrDefault(pp => pp.PlayerId == player1Id);
            var pp2 = play.Playplayers.FirstOrDefault(pp => pp.PlayerId == player2Id);
            if (pp1 is null || pp2 is null) continue;

            var p1SideId = pp1.Role?.SideId ?? 0;
            var p2SideId = pp2.Role?.SideId ?? 0;
            var winnerSideId = play.WinnersideId;

            var p1Won = p1SideId == winnerSideId;
            var p2Won = p2SideId == winnerSideId;

            if (p1Won) totalP1Wins++;
            if (p2Won) totalP2Wins++;

            if (p1SideId != p2SideId)
            {
                oppositeCount++;
                if (winnerSideId == 0) draws++;
                else if (p1SideId == winnerSideId) p1WinsOpposite++;
                else if (p2SideId == winnerSideId) p2WinsOpposite++;
            }
            else if (p1SideId == 1)
            {
                sameMafiaCount++;
                if (winnerSideId == 1) sameMafiaWins++;
                else if (winnerSideId == 2) sameMafiaLosses++;
            }
            else if (p1SideId == 2)
            {
                sameCitizenCount++;
                if (winnerSideId == 2) sameCitizenWins++;
                else if (winnerSideId == 1) sameCitizenLosses++;
            }

            sharedPlayDtos.Add(new SharedPlayDto
            {
                PlayId = play.Id,
                Title = play.Title,
                DateTime = play.DateTime ?? default,
                Player1Side = p1SideId == 1 ? "Mafia" : "Citizen",
                Player2Side = p2SideId == 1 ? "Mafia" : "Citizen",
                WinnerSide = winnerSideId == 1 ? "Mafia" : winnerSideId == 2 ? "Citizen" : null,
                Player1Won = p1Won,
                Player2Won = p2Won
            });
        }

        var p1WinRate = totalShared > 0 ? Math.Round((double)totalP1Wins / totalShared, 2) : 0;
        var p2WinRate = totalShared > 0 ? Math.Round((double)totalP2Wins / totalShared, 2) : 0;

        return new HeadToHeadDto
        {
            Player1 = new PlayerSummaryDto { Id = player1.Id, Name = player1.Name, Picture = player1.Picture },
            Player2 = new PlayerSummaryDto { Id = player2.Id, Name = player2.Name, Picture = player2.Picture },
            TotalSharedPlays = totalShared,
            Player1WinRate = p1WinRate,
            Player2WinRate = p2WinRate,
            OppositeSides = new SideMatchupDto
            {
                Count = oppositeCount,
                Player1Wins = p1WinsOpposite,
                Player2Wins = p2WinsOpposite,
                Draws = draws
            },
            SameSideMafia = new SameSideDto
            {
                Count = sameMafiaCount,
                Wins = sameMafiaWins,
                Losses = sameMafiaLosses
            },
            SameSideCitizen = new SameSideDto
            {
                Count = sameCitizenCount,
                Wins = sameCitizenWins,
                Losses = sameCitizenLosses
            },
            SharedPlays = sharedPlayDtos
        };
    }

    public async Task<PlayListResponseDto> GetPlaysAsync(PlayFilterDto filter)
    {
        var query = _context.Plays
            .Include(p => p.Senario)
            .Include(p => p.Winnerside)
            .Include(p => p.Master)
            .Include(p => p.Event)
                .ThenInclude(e => e.Club)
            .Include(p => p.Playplayers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p =>
                p.Title!.Contains(filter.Search) ||
                p.Master!.Name!.Contains(filter.Search) ||
                p.Event!.Name!.Contains(filter.Search));

        if (filter.ClubId.HasValue)
            query = query.Where(p => p.Event != null && p.Event.ClubId == filter.ClubId);

        if (filter.EventId.HasValue)
            query = query.Where(p => p.EventId == filter.EventId);

        if (filter.SenarioId.HasValue)
            query = query.Where(p => p.SenarioId == filter.SenarioId);

        if (filter.MasterId.HasValue)
            query = query.Where(p => p.MasterId == filter.MasterId);

        if (filter.WinnersideId.HasValue)
            query = query.Where(p => p.WinnersideId == filter.WinnersideId);

        if (filter.PlayerId.HasValue)
            query = query.Where(p =>
                p.Playplayers.Any(pp => pp.PlayerId == filter.PlayerId));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.DateTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new PlayListDto
            {
                Id = p.Id,
                Title = p.Title,
                DateTime = p.DateTime,
                PlayersCount = p.PlayersCount,
                Link = p.Link,
                SenarioName = p.Senario != null ? p.Senario.Name : null,
                MasterName = p.Master != null ? p.Master.Name : null,
                WinnersideName = p.Winnerside != null ? p.Winnerside.Name : null,
                EventName = p.Event != null ? p.Event.Name : null,
                ClubName = p.Event != null && p.Event.Club != null ? p.Event.Club.Name : null,
            })
            .ToListAsync();

        return new PlayListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };
    }

    public async Task<PlayDetailDto?> GetPlayByIdAsync(int playId)
    {
        var play = await _context.Plays
            .Include(p => p.Senario)
            .Include(p => p.Winnerside)
            .Include(p => p.Master)
            .Include(p => p.Event)
                .ThenInclude(e => e.Club)
            .Include(p => p.Playplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Playplayers)
                .ThenInclude(pp => pp.Role)
                    .ThenInclude(r => r.Side)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return null;

        return new PlayDetailDto
        {
            Id = play.Id,
            Title = play.Title,
            DateTime = play.DateTime,
            PlayersCount = play.PlayersCount,
            GuestCount = play.GuestCount,
            Link = play.Link,
            Desc = play.Desc,
            SenarioId = play.SenarioId,
            WinnersideId = play.WinnersideId,
            EventId = play.EventId,
            RoomId = play.RoomId,
            MasterId = play.MasterId,
            SenarioName = play.Senario?.Name,
            WinnersideName = play.Winnerside?.Name,
            MasterName = play.Master?.Name,
            EventName = play.Event?.Name,
            ClubName = play.Event?.Club?.Name,
            Players = play.Playplayers.Select(pp => new PlayDetailPlayerDto
            {
                PlayerId = pp.PlayerId,
                PlayerName = pp.Player?.Name,
                PlayerPicture = pp.Player?.Picture,
                RoleId = pp.RoleId,
                RoleName = pp.Role?.Name,
                SideName = pp.Role?.Side?.Name,
                SideId = pp.Role?.SideId ?? 0,
                Rank = pp.Rank,
                Action = pp.Action
            }).ToList()
        };
    }
}
