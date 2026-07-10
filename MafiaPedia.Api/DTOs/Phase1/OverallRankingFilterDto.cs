namespace MafiaPedia.Api.DTOs.Phase1;

public class OverallRankingFilterDto
{
    public int? ClubId { get; set; }
    public int? EventId { get; set; }
    public int? ScenarioId { get; set; }
    public int MinimumGames { get; set; } = 20;
}