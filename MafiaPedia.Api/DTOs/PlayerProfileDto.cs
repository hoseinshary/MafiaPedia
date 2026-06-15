namespace MafiaPedia.Api.DTOs;

public class PlayerProfileDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public DateOnly? Birthday { get; set; }
    public PlayerStatisticsDto Statistics { get; set; } = null!;
    public List<PlayerRoleSummaryDto> MostPlayedRoles { get; set; } = new();
    public List<PlayerRoleSummaryDto> BestRoles { get; set; } = new();
    public List<PlayerRecentGameDto> RecentGames { get; set; } = new();
    public int WinStreak { get; set; }
    public int BestRun { get; set; }
    public BestPartnerDto? BestMafiaPartner { get; set; }
    public BestPartnerDto? BestCitizenPartner { get; set; }
    public List<WinRateTrendDto> WinRateTrend { get; set; } = new();
}

public class PlayerStatisticsDto
{
    public int TotalGames { get; set; }
    public double OverallWinRate { get; set; }
    public int CitizenGames { get; set; }
    public double CitizenWinRate { get; set; }
    public int MafiaGames { get; set; }
    public double MafiaWinRate { get; set; }
}

public class PlayerRoleSummaryDto
{
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int Games { get; set; }
    public int? Wins { get; set; }
    public double? WinRate { get; set; }
}

public class PlayerRecentGameDto
{
    public int PlayId { get; set; }
    public string? PlayTitle { get; set; }
    public string? RoleName { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Link { get; set; }
}

public class BestPartnerDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public int SharedGames { get; set; }
    public double WinRate { get; set; }
}

public class WinRateTrendDto
{
    public int GameIndex { get; set; }
    public double WinRate { get; set; }
}
