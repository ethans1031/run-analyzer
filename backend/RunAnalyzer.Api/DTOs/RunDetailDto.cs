namespace RunAnalyzer.Api.DTOs;

public class RunDetailDto
{
    public int Id { get; set; }

    public string CharacterName { get; set; } = string.Empty;

    public bool Won { get; set; }

    public int Ascension { get; set; }

    public int FloorReached { get; set; }

    public DateTime TimeStarted { get; set; }

    public int TimeElapsed { get; set; }

    public string? KilledByEncounter { get; set; }

    public string BuildId { get; set; } = string.Empty;

    public string Seed { get; set; } = string.Empty;

    public string GameMode { get; set; } = string.Empty;

    public int SchemaVersion { get; set; }

    public List<CardChoiceDto> CardChoices { get; set; } = new();

    public List<RelicChoiceDto> RelicChoices { get; set; } = new();
}
