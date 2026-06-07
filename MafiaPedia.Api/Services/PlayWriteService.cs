using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MafiaPedia.Api.Services;

public class PlayWriteService : IPlayWriteService
{
    private readonly MafiaDbContext _context;

    public PlayWriteService(MafiaDbContext context)
    {
        _context = context;
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
