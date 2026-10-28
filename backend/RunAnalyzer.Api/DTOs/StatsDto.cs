namespace RunAnalyzer.Api.DTOs;

public class RunStatsDto
{
    public int TotalRuns { get; set; }

    public int TotalWins { get; set; }

    public int TotalLosses { get; set; }

    public double OverallWinRate { get; set; }

    public double AverageFloorReached { get; set; }

    /// <summary>Average length of winning runs in seconds, or null if there are none.</summary>
    public double? AverageWinTime { get; set; }

    /// <summary>Average length of losing runs in seconds, or null if there are none.</summary>
    public double? AverageLossTime { get; set; }

    /// <summary>Most consecutive wins, by start time.</summary>
    public int LongestWinStreak { get; set; }

    public List<CharacterStatsDto> ByCharacter { get; set; } = new();

    public List<AscensionStatsDto> ByAscension { get; set; } = new();

    /// <summary>Encounters that ended the most runs, most common first.</summary>
    public List<KillerStatsDto> TopKillers { get; set; } = new();

    /// <summary>Losses grouped into floor ranges, with empty ranges included.</summary>
    public List<FloorBucketDto> LossesByFloor { get; set; } = new();

    /// <summary>Runs and win rate per calendar week (weeks start on Monday).</summary>
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
