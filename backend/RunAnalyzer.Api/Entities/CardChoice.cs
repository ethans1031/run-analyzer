namespace RunAnalyzer.Api.Entities;

/// <summary>
/// One card offered to the player on one floor of a run, picked or skipped.
/// </summary>
public class CardChoice
{
    public int Id { get; set; }

    public int RunId { get; set; }

    /// <summary>Game card ID, e.g. "CARD.BOOST_AWAY".</summary>
    public string CardId { get; set; } = string.Empty;

    public bool Picked { get; set; }

    /// <summary>Zero-based act index.</summary>
    public int Act { get; set; }

    /// <summary>Zero-based floor index within the act.</summary>
    public int Floor { get; set; }

    /// <summary>Where the offer came from: combat, elite, boss, shop, event, ancient, treasure.</summary>
    public string Source { get; set; } = string.Empty;
}
