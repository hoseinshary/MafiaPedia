namespace MafiaPedia.Api.DTOs.Phase2.Event;

public record EventDto(int Id, string Name, int ClubId, bool IsDefault);
public record CreateEventDto(string Name, bool SetAsDefault = false);
