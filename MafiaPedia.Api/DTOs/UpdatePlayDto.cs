namespace MafiaPedia.Api.DTOs;

public class UpdatePlayDto
{
    public string? Title { get; set; }
    public DateTime? DateTime { get; set; }
    public int? PlayersCount { get; set; }
    public string? Desc { get; set; }
    public int? SenarioId { get; set; }
    public int? WinnersideId { get; set; }
    public int? EventId { get; set; }
    public int? RoomId { get; set; }
    public int? MasterId { get; set; }
    public int? GuestCount { get; set; }
    public string? Link { get; set; }
    public List<CreatePlayPlayerDto>? Players { get; set; }
}
