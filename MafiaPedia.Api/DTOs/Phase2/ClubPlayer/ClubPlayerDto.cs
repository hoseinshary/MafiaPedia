namespace MafiaPedia.Api.DTOs.Phase2.ClubPlayer;

public record ClubPlayerDto(
    int Id, string Name, string Mobile,
    DateTime? Birthday, string? Code, string? Picture, string? Desc,
    DateTime? JoinedAt // null when not club-scoped
);

public record CreateOrJoinClubPlayerDto(
    string Name, string Mobile, DateTime? Birthday, string? Code, string? Desc
);

public record ClubPlayerJoinResultDto(ClubPlayerDto ClubPlayer, bool WasExistingCustomer);

public record UpdateClubPlayerDto(string? Name, DateTime? Birthday, string? Code, string? Desc);

public record CustomerSearchResultDto(
    List<ClubPlayerDto> InClub,
    List<ClubPlayerDto> GlobalOthers
);
