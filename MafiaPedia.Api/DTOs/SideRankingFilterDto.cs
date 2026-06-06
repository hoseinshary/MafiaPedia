using System.ComponentModel.DataAnnotations;

namespace MafiaPedia.Api.DTOs;

public class SideRankingFilterDto
{
    [Required]
    public int SideId { get; set; }
    public int? ClubId { get; set; }
    public int? EventId { get; set; }
    public int? ScenarioId { get; set; }
    public int MinimumGames { get; set; } = 10;
}
