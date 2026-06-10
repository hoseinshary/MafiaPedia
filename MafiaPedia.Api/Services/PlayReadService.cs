using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public class PlayReadService : IPlayReadService
{
    private readonly MafiaDbContext _context;

    public PlayReadService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<PlayListResponseDto> GetPlaysAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _context.Plays
            .Include(p => p.Senario)
            .Include(p => p.Winnerside)
            .Include(p => p.Event)
            .Include(p => p.Master)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                (p.Title != null && p.Title.Contains(term)) ||
                (p.Event.Name != null && p.Event.Name.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlayListDto
            {
                Id = p.Id,
                Title = p.Title,
                DateTime = p.DateTime,
                PlayersCount = p.PlayersCount,
                Link = p.Link,
                SenarioName = p.Senario.Name,
                MasterName = p.Master.Name,
                WinnersideName = p.Winnerside.Name,
                EventName = p.Event.Name
            })
            .ToListAsync();

        return new PlayListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PlayDetailDto?> GetPlayByIdAsync(int playId)
    {
        var play = await _context.Plays
            .Include(p => p.Playplayers)
                .ThenInclude(pp => pp.Player)
            .Include(p => p.Playplayers)
                .ThenInclude(pp => pp.Role)
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
            Players = play.Playplayers.Select(pp => new PlayDetailPlayerDto
            {
                PlayerId = pp.PlayerId,
                PlayerName = pp.Player?.Name,
                RoleId = pp.RoleId,
                RoleName = pp.Role?.Name,
                Rank = pp.Rank,
                Action = pp.Action
            }).ToList()
        };
    }
}
