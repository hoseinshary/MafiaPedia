namespace MafiaPedia.Api.DTOs.Phase1;

public class PlayListDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime? DateTime { get; set; }
    public int? PlayersCount { get; set; }
    public string? Link { get; set; }
    public string? SenarioName { get; set; }
    public string? MasterName { get; set; }
    public string? WinnersideName { get; set; }
    public string? EventName { get; set; }
    public string? ClubName { get; set; }
}

public class PlayFilterDto
{
    public int? ClubId { get; set; }
    public int? EventId { get; set; }
    public int? SenarioId { get; set; }
    public int? PlayerId { get; set; }
    public int? MasterId { get; set; }
    public int? WinnersideId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PlayListResponseDto
{
    public List<PlayListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PlayDetailDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime? DateTime { get; set; }
    public int? PlayersCount { get; set; }
    public int? GuestCount { get; set; }
    public string? Link { get; set; }
    public string? Desc { get; set; }

    // IDs (for edit form)
    public int SenarioId { get; set; }
    public int WinnersideId { get; set; }
    public int EventId { get; set; }
    public int RoomId { get; set; }
    public int MasterId { get; set; }

    // Names (for display)
    public string? SenarioName { get; set; }
    public string? WinnersideName { get; set; }
    public string? MasterName { get; set; }
    public string? EventName { get; set; }
    public string? ClubName { get; set; }

    public List<PlayDetailPlayerDto> Players { get; set; } = new();
}

public class PlayDetailPlayerDto
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public string? PlayerPicture { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? SideName { get; set; }
    public int SideId { get; set; }
    public int? Rank { get; set; }
    public int? Action { get; set; }
}
