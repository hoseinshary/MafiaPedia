using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;
using Microsoft.EntityFrameworkCore;

namespace MafiaPedia.Api.Services.Phase1;

public class PlayWriteService : IPlayWriteService
{
    private readonly MafiaDbContext _context;

    public PlayWriteService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdatePlayAsync(int playId, UpdatePlayDto dto)
    {
        var play = await _context.Plays
            .Include(p => p.Playplayers)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null) return false;

        if (dto.Title != null) play.Title = dto.Title;
        if (dto.DateTime != null) play.DateTime = dto.DateTime;
        if (dto.PlayersCount != null) play.PlayersCount = dto.PlayersCount;
        if (dto.Desc != null) play.Desc = dto.Desc;
        if (dto.SenarioId != null) play.SenarioId = dto.SenarioId.Value;
        if (dto.WinnersideId != null) play.WinnersideId = dto.WinnersideId.Value;
        if (dto.EventId != null) play.EventId = dto.EventId.Value;
        if (dto.RoomId != null) play.RoomId = dto.RoomId.Value;
        if (dto.MasterId != null) play.MasterId = dto.MasterId.Value;
        if (dto.GuestCount != null) play.GuestCount = dto.GuestCount;
        if (dto.Link != null) play.Link = dto.Link;

        if (dto.Players != null)
        {
            _context.Playplayers.RemoveRange(play.Playplayers);

            var newPlayers = dto.Players.Select(p => new Playplayer
            {
                PlayId = playId,
                PlayerId = p.PlayerId,
                RoleId = p.RoleId,
                Action = p.Action,
                Rank = p.Rank
            }).ToList();

            await _context.Playplayers.AddRangeAsync(newPlayers);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeletePlayAsync(int playId)
    {
        var play = await _context.Plays
            .Include(p => p.Playplayers)
            .FirstOrDefaultAsync(p => p.Id == playId);

        if (play is null)
            return (false, "بازی یافت نشد");

        _context.Playplayers.RemoveRange(play.Playplayers);
        _context.Plays.Remove(play);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<int> AddPlayAsync(CreatePlayDto dto)
    {
        var play = new Play
        {
            Title = dto.Title,
            DateTime = dto.DateTime,
            PlayersCount = dto.PlayersCount,
            Desc = dto.Desc,
            SenarioId = dto.SenarioId,
            WinnersideId = dto.WinnersideId,
            EventId = dto.EventId,
            RoomId = dto.RoomId,
            MasterId = dto.MasterId,
            UserId = dto.UserId,
            GuestCount = dto.GuestCount,
            Link = dto.Link
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.Plays.Add(play);
        await _context.SaveChangesAsync();

        foreach (var playerDto in dto.Players)
        {
            var playplayer = new Playplayer
            {
                PlayId = play.Id,
                PlayerId = playerDto.PlayerId,
                RoleId = playerDto.RoleId,
                Action = playerDto.Action,
                Rank = playerDto.Rank
            };
            _context.Playplayers.Add(playplayer);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return play.Id;
    }
}
