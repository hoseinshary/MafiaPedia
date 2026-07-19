namespace MafiaPedia.Api.DTOs.Phase1;

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
    public string? PlayersJson { get; set; }
    public IFormFile? Picture { get; set; }
}
