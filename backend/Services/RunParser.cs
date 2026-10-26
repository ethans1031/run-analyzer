using System.Text.Json;
using backend.Entities;

namespace backend.Services;

public class RunParser
{
    // NEW: map_point_type -> Source value for CardChoice/RelicChoice rows.
    // Unknown types fall back to the raw map_point_type string.
    private static readonly Dictionary<string, string> SourceByMapPointType = new()
    {
        ["monster"] = "combat",
        ["elite"] = "elite",
        ["boss"] = "boss",
        ["shop"] = "shop",
        ["treasure"] = "treasure",
        ["ancient"] = "ancient",
        ["unknown"] = "event",
        // rest_site intentionally omitted — never has choices
    };

    public Run Parse(string rawJson)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(rawJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidRunFileException("File is not valid JSON.", ex);
        }

        var root = doc.RootElement;

        // Helpers for required scalars at root.
        string GetString(string name) =>
            root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString()!
                : throw new InvalidRunFileException($"Missing or invalid string field: {name}");

        int GetInt(string name) =>
            root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number
                ? p.GetInt32()
                : throw new InvalidRunFileException($"Missing or invalid int field: {name}");

        bool GetBool(string name) =>
            root.TryGetProperty(name, out var p) && (p.ValueKind == JsonValueKind.True || p.ValueKind == JsonValueKind.False)
                ? p.GetBoolean()
                : throw new InvalidRunFileException($"Missing or invalid bool field: {name}");

        string? Nullable(string s) => s == "NONE.NONE" ? null : s;

        var run = new Run
        {
            Character = "",
            Won = GetBool("win"),
            Ascension = GetInt("ascension"),
            TimeElapsed = GetInt("run_time"),
            TimeStarted = DateTimeOffset.FromUnixTimeSeconds(GetInt("start_time")).UtcDateTime,
            Seed = GetString("seed"),
            BuildId = GetString("build_id"),
            GameMode = GetString("game_mode"),
            KilledByEncounter = Nullable(GetString("killed_by_encounter")),
            SchemaVersion = GetInt("schema_version"),
            FloorReached = 0,
            RunJson = rawJson,
        };

        // Character (nested under players[0])
        if (!root.TryGetProperty("players", out var players)
            || players.ValueKind != JsonValueKind.Array
            || players.GetArrayLength() == 0)
        {
            throw new InvalidRunFileException("Missing or empty players array.");
        }
        var firstPlayer = players[0];
        if (!firstPlayer.TryGetProperty("character", out var charProp) || charProp.ValueKind != JsonValueKind.String)
        {
            throw new InvalidRunFileException("Missing players[0].character.");
        }
        run.Character = charProp.GetString()!;

        // FloorReached + CardChoices walk in one pass over map_point_history.
        if (root.TryGetProperty("map_point_history", out var mph) && mph.ValueKind == JsonValueKind.Array)
        {
            int totalFloors = 0;
            int actIndex = 0;
            foreach (var act in mph.EnumerateArray())
            {
                if (act.ValueKind != JsonValueKind.Array)
                {
                    actIndex++;
                    continue;
                }

                int floorIndex = 0;
                foreach (var mapPoint in act.EnumerateArray())
                {
                    totalFloors++;
                    ExtractCardChoices(mapPoint, actIndex, floorIndex, run.CardChoices);
                    ExtractRelicChoices(mapPoint, actIndex, floorIndex, run.RelicChoices);
                    floorIndex++;
                }
                actIndex++;
            }
            run.FloorReached = totalFloors;
        }

        return run;
    }

    // NEW: walks a single map point and appends a CardChoice row per offered card.
    private static void ExtractCardChoices(
        JsonElement mapPoint,
        int actIndex,
        int floorIndex,
        List<CardChoice> output)
    {
        if (!mapPoint.TryGetProperty("map_point_type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            return;  // malformed floor; skip rather than fail the whole run

        var mapType = typeProp.GetString()!;
        var source = SourceByMapPointType.TryGetValue(mapType, out var mapped) ? mapped : mapType;

        if (!mapPoint.TryGetProperty("player_stats", out var playerStats)
            || playerStats.ValueKind != JsonValueKind.Array
            || playerStats.GetArrayLength() == 0)
            return;

        var ps = playerStats[0];

        if (!ps.TryGetProperty("card_choices", out var cardChoices) || cardChoices.ValueKind != JsonValueKind.Array)
            return;

        foreach (var choice in cardChoices.EnumerateArray())
        {
            if (!choice.TryGetProperty("card", out var card)
                || !card.TryGetProperty("id", out var idProp)
                || idProp.ValueKind != JsonValueKind.String)
                continue;  // skip malformed entry

            bool picked = choice.TryGetProperty("was_picked", out var wp)
                          && wp.ValueKind == JsonValueKind.True;

            output.Add(new CardChoice
            {
                CardId = idProp.GetString()!,
                Picked = picked,
                Act = actIndex,
                Floor = floorIndex,
                Source = source,
            });
        }
    }

    private static void ExtractRelicChoices(
    JsonElement mapPoint,
    int actIndex,
    int floorIndex,
    List<RelicChoice> output)
    {
        if (!mapPoint.TryGetProperty("map_point_type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            return;

        var mapType = typeProp.GetString()!;
        var source = SourceByMapPointType.TryGetValue(mapType, out var mapped) ? mapped : mapType;

        if (!mapPoint.TryGetProperty("player_stats", out var playerStats)
            || playerStats.ValueKind != JsonValueKind.Array
            || playerStats.GetArrayLength() == 0)
            return;

        var ps = playerStats[0];

        if (!ps.TryGetProperty("relic_choices", out var relicChoices) || relicChoices.ValueKind != JsonValueKind.Array)
            return;

        foreach (var choice in relicChoices.EnumerateArray())
        {
            if (!choice.TryGetProperty("choice", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                continue;

            bool picked = choice.TryGetProperty("was_picked", out var wp)
                          && wp.ValueKind == JsonValueKind.True;

            output.Add(new RelicChoice
            {
                RelicId = idProp.GetString()!,
                Picked = picked,
                Act = actIndex,
                Floor = floorIndex,
                Source = source,
            });
        }
    }

}

public class InvalidRunFileException : Exception
{
    public InvalidRunFileException(string message) : base(message) { }
    public InvalidRunFileException(string message, Exception inner) : base(message, inner) { }
}