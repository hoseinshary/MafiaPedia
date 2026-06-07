namespace MafiaPedia.Api.DTOs;

public class CreatePlayDto
{
    public string? Title { get; set; }
    public DateTime? DateTime { get; set; }
    public int? PlayersCount { get; set; }
    public string? Desc { get; set; }
    public int SenarioId { get; set; }
    public int WinnersideId { get; set; }
    public int EventId { get; set; }
    public int RoomId { get; set; } = 1;
    public int MasterId { get; set; } = 1;
    public int UserId { get; set; } = 1;
    public int? GuestCount { get; set; }
    public string? Link { get; set; }
    public List<CreatePlayPlayerDto> Players { get; set; } = new();
}

public class CreatePlayPlayerDto
{
    public int PlayerId { get; set; }
    public int RoleId { get; set; }
    public int? Action { get; set; }
    public int? Rank { get; set; }
}
