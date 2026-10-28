namespace RunAnalyzer.Api.DTOs;

/// <summary>Per-card or per-relic stats plus the baseline they're compared against.</summary>
public class ChoiceStatsResponseDto
{
    /// <summary>Runs included (all runs, or one character's runs).</summary>
    public int RunCount { get; set; }

    /// <summary>Win rate across those runs; each item's win rate is compared to this.</summary>
    public double BaselineWinRate { get; set; }

    public List<ChoiceStatsDto> Items { get; set; } = new();
}

/// <summary>Stats for one card or relic ID.</summary>
public class ChoiceStatsDto
{
    /// <summary>Game ID, e.g. "CARD.BOOST_AWAY" or "RELIC.ANCHOR".</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Times offered where taking it was a real choice (reward screens for cards, shops for relics).</summary>
    public int TimesOffered { get; set; }

    /// <summary>Times taken out of <see cref="TimesOffered"/>.</summary>
    public int TimesPicked { get; set; }

    /// <summary>TimesPicked / TimesOffered, or null if never offered as a choice.</summary>
    public double? PickRate { get; set; }

    /// <summary>Distinct runs where it was picked from any source.</summary>
    public int RunsPicked { get; set; }

    /// <summary>How many of those runs were won.</summary>
    public int RunsWon { get; set; }

    /// <summary>RunsWon / RunsPicked, or null if never picked.</summary>
    public double? WinRate { get; set; }

    /// <summary>WinRate minus the baseline win rate, or null if never picked.</summary>
    public double? WinRateDelta { get; set; }
}
