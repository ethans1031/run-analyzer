//Used for list of runs table
namespace backend.DTOs;

public class RunSummaryDto
{
    public int Id { get; set; }

    public string CharacterName { get; set; } = string.Empty;

    public bool Won { get; set; }

    public int Ascension { get; set; }

    public int FloorReached { get; set; }

    public DateTime TimeStarted { get; set; }

    public int TimeElapsed { get; set; }
}