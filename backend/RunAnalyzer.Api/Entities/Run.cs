namespace RunAnalyzer.Api.Entities;

/// <summary>
/// One completed Slay the Spire 2 run. Scalar columns are extracted from the
/// run file for querying; <see cref="RunJson"/> keeps the original file so new
/// fields can be re-parsed later without re-uploading.
/// </summary>
public class Run
{
    public int Id { get; set; }

    public string Character { get; set; } = string.Empty;

    public int Ascension { get; set; }

    public bool Won { get; set; }

    /// <summary>Run length in seconds.</summary>
    public int TimeElapsed { get; set; }

    public int FloorReached { get; set; }

    public DateTime TimeStarted { get; set; }

    public string RunJson { get; set; } = string.Empty;

    public string BuildId { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public string Seed { get; set; } = string.Empty;

    /// <summary>Encounter that ended the run, or null for a win.</summary>
    public string? KilledByEncounter { get; set; }

    public int SchemaVersion { get; set; }

    public List<CardChoice> CardChoices { get; set; } = new();

    public List<RelicChoice> RelicChoices { get; set; } = new();
}
