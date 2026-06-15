namespace MafiaPedia.Api.DTOs;

public class OverallRankingFilterDto
{
    public int? ClubId { get; set; }
    public int? EventId { get; set; }
    public int? ScenarioId { get; set; }
    public int MinimumGames { get; set; } = 20;
}