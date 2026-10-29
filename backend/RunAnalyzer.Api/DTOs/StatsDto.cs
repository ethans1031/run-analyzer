namespace RunAnalyzer.Api.DTOs;

public class RunStatsDto
{
    public int TotalRuns { get; set; }

    public int TotalWins { get; set; }

    public int TotalLosses { get; set; }

    public double OverallWinRate { get; set; }

    public double AverageFloorReached { get; set; }

    public double? AverageWinTime { get; set; }

    public double? AverageLossTime { get; set; }

    public int LongestWinStreak { get; set; }

    public List<CharacterStatsDto> ByCharacter { get; set; } = new();

    public List<AscensionStatsDto> ByAscension { get; set; } = new();

    public List<KillerStatsDto> TopKillers { get; set; } = new();

    public List<FloorBucketDto> LossesByFloor { get; set; } = new();

    public List<WeeklyStatsDto> ByWeek { get; set; } = new();
}

public class CharacterStatsDto
{
    public string Character { get; set; } = string.Empty;

    public int RunCount { get; set; }

    public int WinCount { get; set; }

    public double WinRate { get; set; }

    public double AverageFloorReached { get; set; }
}

public class AscensionStatsDto
{
    public int Ascension { get; set; }

    public int RunCount { get; set; }

    public int WinCount { get; set; }

    public double WinRate { get; set; }
}

public class KillerStatsDto
{
    public string Encounter { get; set; } = string.Empty;

    public int Count { get; set; }

    /// <summary>Count divided by total losses.</summary>
    public double ShareOfLosses { get; set; }
}

public class FloorBucketDto
{
    public int FloorStart { get; set; }

    public int FloorEnd { get; set; }

    public int Losses { get; set; }
}

public class WeeklyStatsDto
{
    public DateOnly WeekStart { get; set; }

    public int RunCount { get; set; }

    public int WinCount { get; set; }

    public double WinRate { get; set; }
}
