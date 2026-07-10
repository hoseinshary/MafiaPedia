using MafiaPedia.Api.DTOs.Phase2.Event;

namespace MafiaPedia.Api.IServices.Phase2;

public interface IEventService
{
    Task<List<EventDto>> GetClubEventsAsync(int clubId);
    Task<EventDto> CreateEventAsync(int clubId, CreateEventDto dto);
    Task<EventDto> SetDefaultEventAsync(int clubId, int eventId);
    Task<EventDto?> GetDefaultEventAsync(int clubId);
}
