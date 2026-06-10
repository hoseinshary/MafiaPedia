namespace MafiaPedia.Api.DTOs;

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
    public int SenarioId { get; set; }
    public int WinnersideId { get; set; }
    public int EventId { get; set; }
    public int RoomId { get; set; }
    public int MasterId { get; set; }
    public List<PlayDetailPlayerDto> Players { get; set; } = new();
}

public class PlayDetailPlayerDto
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? Rank { get; set; }
    public int? Action { get; set; }
}
