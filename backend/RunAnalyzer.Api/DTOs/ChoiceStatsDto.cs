namespace RunAnalyzer.Api.DTOs;

public class ChoiceStatsResponseDto
{
    public int RunCount { get; set; }

    public double BaselineWinRate { get; set; }

    public List<ChoiceStatsDto> Items { get; set; } = new();
}

public class ChoiceStatsDto
{
    public string Id { get; set; } = string.Empty;

    public int TimesOffered { get; set; }

    public int TimesPicked { get; set; }

    public double? PickRate { get; set; }

    public int RunsPicked { get; set; }

    public int RunsWon { get; set; }

    public double? WinRate { get; set; }

    public double? WinRateDelta { get; set; }
}
