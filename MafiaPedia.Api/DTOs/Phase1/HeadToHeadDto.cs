namespace MafiaPedia.Api.DTOs.Phase1;

public class HeadToHeadDto
{
    public PlayerSummaryDto Player1 { get; set; } = null!;
    public PlayerSummaryDto Player2 { get; set; } = null!;
    public int TotalSharedPlays { get; set; }
    public double Player1WinRate { get; set; }
    public double Player2WinRate { get; set; }
    public SideMatchupDto OppositeSides { get; set; } = null!;
    public SameSideDto SameSideMafia { get; set; } = null!;
    public SameSideDto SameSideCitizen { get; set; } = null!;
    public List<SharedPlayDto> SharedPlays { get; set; } = new();
}

public class PlayerSummaryDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
}

public class SideMatchupDto
{
    public int Count { get; set; }
    public int Player1Wins { get; set; }
    public int Player2Wins { get; set; }
    public int Draws { get; set; }
}

public class SameSideDto
{
    public int Count { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}

public class SharedPlayDto
{
    public int PlayId { get; set; }
    public string? Title { get; set; }
    public DateTime DateTime { get; set; }
    public string? Player1Side { get; set; }
    public string? Player2Side { get; set; }
    public string? WinnerSide { get; set; }
    public bool Player1Won { get; set; }
    public bool Player2Won { get; set; }
}