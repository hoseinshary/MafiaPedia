using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase2.Event;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Services.Phase2;

public class EventService : IEventService
{
    private readonly MafiaDbContext _context;

    public EventService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventDto>> GetClubEventsAsync(int clubId)
    {
        return await _context.Events
            .Where(e => e.ClubId == clubId)
            .OrderByDescending(e => e.Id)
            .Select(e => new EventDto(e.Id, e.Name ?? "", e.ClubId, e.IsDefault))
            .ToListAsync();
    }

    public async Task<EventDto> CreateEventAsync(int clubId, CreateEventDto dto)
    {
        var clubExists = await _context.Clubs.AnyAsync(c => c.Id == clubId);
        if (!clubExists)
            throw new KeyNotFoundException("باشگاه یافت نشد");

        var eventEntity = new Entities.Event
        {
            Name = dto.Name,
            ClubId = clubId,
            IsDefault = false
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        if (dto.SetAsDefault)
        {
            return await SetDefaultEventAsync(clubId, eventEntity.Id);
        }

        return new EventDto(eventEntity.Id, eventEntity.Name ?? "", eventEntity.ClubId, eventEntity.IsDefault);
    }

    public async Task<EventDto> SetDefaultEventAsync(int clubId, int eventId)
    {
        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.ClubId == clubId);
        if (eventEntity is null)
            throw new KeyNotFoundException("رویداد یافت نشد");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var previousDefaults = await _context.Events
            .Where(e => e.ClubId == clubId && e.IsDefault)
            .ToListAsync();

        foreach (var prev in previousDefaults)
        {
            prev.IsDefault = false;
        }

        eventEntity.IsDefault = true;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new EventDto(eventEntity.Id, eventEntity.Name ?? "", eventEntity.ClubId, eventEntity.IsDefault);
    }

    public async Task<EventDto?> GetDefaultEventAsync(int clubId)
    {
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.ClubId == clubId && e.IsDefault);

        if (eventEntity is null) return null;

        return new EventDto(eventEntity.Id, eventEntity.Name ?? "", eventEntity.ClubId, eventEntity.IsDefault);
    }
}
