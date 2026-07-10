namespace MafiaPedia.Api.DTOs.Phase1;

public class SideRankingDto
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
    public string? Picture { get; set; }

}
