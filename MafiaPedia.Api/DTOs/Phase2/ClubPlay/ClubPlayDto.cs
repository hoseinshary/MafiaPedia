namespace MafiaPedia.Api.DTOs.Phase2.ClubPlay;

public record ParticipantInputDto(int ClubPlayerId, bool IsGuest, int EntryCount = 1);

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
    int? NerkhId,
    List<ParticipantInputDto> Participants,
    bool ShuffleRoles = true,
    int? MasterId = null
);

public record ClubPlayParticipantDto(
    int Id, int ClubPlayerId, string Name, int RoleId, string RoleName, int SideId, string? RolePhoto, bool IsGuest, int EntryCount
);

public record ClubPlayDetailDto(
    int Id, string? Title, DateTime DateTime, int RoomId, string RoomName,
    int SenarioId, string SenarioName, int PlayersCount, int GuestCount,
    string? Desc, string? Link, string PlayType, string Status,
    int MasterId, string MasterName, int? WinnersideId,
    int EventId, string EventName,
    int? NerkhId, string? NerkhName,
    List<ClubPlayParticipantDto> Participants
);

public record ReplaceParticipantDto(int NewClubPlayerId, bool IsGuest, int EntryCount = 1);

public record ParticipantRankDto(int Id, int Rank);

public record ClubPlayListItemDto(
    int Id, string? Title, DateTime DateTime, DateOnly BusinessDate,
    string RoomName, string SenarioName, int PlayersCount, int GuestCount,
    string Status, string PlayType,
    string? MasterName = null
);

public record ClubPlayDeletedListItemDto(
    int Id, string? Title, DateTime DateTime, DateOnly BusinessDate,
    string RoomName, string SenarioName, int PlayersCount, int GuestCount,
    string Status, string PlayType,
    string? MasterName,
    DateTime? DeletedAt,
    int? DeletedByUserId,
    string? DeletedByDisplayName
);

public record MasterStatsDto(int TotalPlays, int TotalEntries, int TotalGuestEntries);

public record MasterPerformanceDto(int MasterId, string MasterName, int PlayCount, int EntryCount, int GuestEntryCount);

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
    int? NerkhId,
    List<ParticipantInputDto> Participants
);
