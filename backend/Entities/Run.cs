//Class that represents a run in slay the spire 2 in the database

namespace backend.Entities;

//  Id (int) primary key
//  Ascension (int)
//  Character (string)
//  Won (boolean)
//  TimeElapsed (int in seconds)
//  TimeStarted (DateTime)
//  FloorReached (int)
//  RunJson (string)
//  BuildId (string)
//  GameMode (String)
//  Seed (String)
//  KilledByEncounter (string, nullable)
//  SchemaVersion (int)

//  CardChoices List<CardChoice> 
//  RelicChoices List<RelicChoice> 
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

    public string? KilledByEncounter { get; set; } = string.Empty;

    public int SchemaVersion { get; set; }

    public List<CardChoice> CardChoices { get; set; } = new();      //navigation property

    public List<RelicChoice> RelicChoices { get; set; } = new();



    
}