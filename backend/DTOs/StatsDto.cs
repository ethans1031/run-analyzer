namespace backend.DTOs;

public class RunStatsDto
{
    public int TotalRuns { get; set; }

    public int TotalWins { get; set; }

    public int TotalLosses { get; set; }

    public double OverallWinRate { get; set; }

    public double AverageFloorReached { get; set; }

    public List<CharacterStatsDto> ByCharacter { get; set; } = new();

    public List<AscensionStatsDto> ByAscension { get; set; } = new();
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