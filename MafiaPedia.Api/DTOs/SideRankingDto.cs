namespace MafiaPedia.Api.DTOs;

public class SideRankingDto
{
    public int PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
}
