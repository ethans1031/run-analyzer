namespace RunAnalyzer.Api.Entities;

public class Run
{
    public int Id { get; set; }

    public string Character { get; set; } = string.Empty;

    public int Ascension { get; set; }

    public bool Won { get; set; }

    public int TimeElapsed { get; set; }

    public int FloorReached { get; set; }

    public DateTime TimeStarted { get; set; }

    public string RunJson { get; set; } = string.Empty;

    public string BuildId { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public string Seed { get; set; } = string.Empty;

    public string? KilledByEncounter { get; set; }

    public int SchemaVersion { get; set; }

    public List<CardChoice> CardChoices { get; set; } = new();

    public List<RelicChoice> RelicChoices { get; set; } = new();
}
