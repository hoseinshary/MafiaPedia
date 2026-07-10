namespace MafiaPedia.Api.DTOs.Phase1;

public class RankingDto
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int TotalGames { get; set; }
    public double OverallWinRate { get; set; }
    public double CitizenWinRate { get; set; }
    public double MafiaWinRate { get; set; }
    public string? Picture { get; set; }

}
