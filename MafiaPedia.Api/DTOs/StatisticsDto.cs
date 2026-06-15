namespace MafiaPedia.Api.DTOs;

public class StatisticsFilterDto
{
    public int? ClubId { get; set; }
    public int? EventId { get; set; }
    public int? ScenarioId { get; set; }
}

public class StatisticsDto
{
    public int TotalGames { get; set; }
    public int TotalPlayers { get; set; }
    public List<SideWinRateDto> WinRateByClub { get; set; } = new();
    public List<SideWinRateDto> WinRateByEvent { get; set; } = new();
    public List<SideWinRateDto> WinRateByScenario { get; set; } = new();
    public List<MonthlyWinRateTrendDto> WinRateTrend { get; set; } = new();
}

public class StatisticsHomeDto
{
    public int TotalGames { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalSenarios { get; set; }
    public int TotalEvents { get; set; }

    public List<SideRankingDto> CitizenTop3Player { get; set; } = new ();
    public List<SideRankingDto> MafiaTop3Player { get; set; } = new(); 
    public List<RankingDto> AllTop3Player { get; set; } = new();
    
    public List<PlayListDto> Last5Plays { get; set; } = new();

    public ClubStatDto DonclubStat { get; set; } = new(); //clubid = 1 
    public ClubStatDto LegendaryStat { get; set; } = new(); // clubid = 2
}

public class ClubStatDto
{
    public int ClubId { get; set; }
    public string ClubName { get; set; } = "";

    public int PlayerCount { get; set; }

    public int PlayCount { get; set; }

    public double MafiaWinRate { get; set; }
}

public class SideWinRateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalGames { get; set; }
    public double MafiaWinRate { get; set; }
    public double CitizenWinRate { get; set; }
}

public class MonthlyWinRateTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalGames { get; set; }
    public double MafiaWinRate { get; set; }
    public double CitizenWinRate { get; set; }
}