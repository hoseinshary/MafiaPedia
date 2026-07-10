namespace MafiaPedia.Api.DTOs.Phase2.ClubPlay;

public record ParticipantInputDto(int ClubPlayerId, bool IsGuest);

public record CreateClubPlayDto(
    string? Title,
    DateTime DateTime,
    int RoomId,
    int SenarioId,
    int PlayersCount,
    string? Desc,
    string? Link,
    string PlayType,
    int? EventId,
    List<ParticipantInputDto> Participants,
    bool ShuffleRoles = true
);

public record ClubPlayParticipantDto(
    int ClubPlayerId, string Name, int RoleId, string RoleName, int SideId, string? RolePhoto, bool IsGuest
);

public record ClubPlayDetailDto(
    int Id, string? Title, DateTime DateTime, int RoomId, string RoomName,
    int SenarioId, string SenarioName, int PlayersCount, int GuestCount,
    string? Desc, string? Link, string PlayType, string Status,
    int MasterId, string MasterName, int? WinnersideId,
    int EventId, string EventName,
    List<ClubPlayParticipantDto> Participants
);

public record ParticipantRankDto(int ClubPlayerId, int Rank);

public record ClubPlayListItemDto(
    int Id, string? Title, DateTime DateTime, DateOnly BusinessDate,
    string RoomName, string SenarioName, int PlayersCount, int GuestCount,
    string Status, string PlayType
);

public record MasterStatsDto(int TotalPlays, int TotalEntries, int TotalGuestEntries);

public record SubmitWinnersideRequestDto(int WinnersideId);

public record UpdateClubPlayDto(
    string? Title,
    DateTime DateTime,
    int RoomId,
    int SenarioId,
    string? Desc,
    string? Link,
    string PlayType,
    int EventId,
    bool ShuffleRoles,
    List<ParticipantInputDto> Participants
);
