using System.Text.Json;
using RunAnalyzer.Api.Entities;

namespace RunAnalyzer.Api.Services;

/// <summary>
/// Turns the raw JSON of a Slay the Spire 2 .run file into a <see cref="Run"/>
/// with its <see cref="CardChoice"/> and <see cref="RelicChoice"/> rows filled in.
/// Has no HTTP or database dependencies, so it can be tested on its own.
/// </summary>
/// <remarks>
/// Missing or invalid top-level fields throw <see cref="InvalidRunFileException"/>,
/// because the file isn't a usable run. A malformed individual floor or choice is
/// skipped instead, so one odd entry doesn't reject the whole run.
/// </remarks>
public class RunParser
{
    // Maps the file's map_point_type to the Source stored on each choice row.
    // Source lets stats separate real choices (combat rewards, shops) from
    // other ways a card or relic shows up. Unknown types keep their raw value.
    private static readonly Dictionary<string, string> SourceByMapPointType = new()
    {
        ["monster"] = "combat",
        ["elite"] = "elite",
        ["boss"] = "boss",
        ["shop"] = "shop",
        ["treasure"] = "treasure",
        ["ancient"] = "ancient",
        ["unknown"] = "event",
        // TODO: need to add rest sites as special cases for certain relics (dream catcher, shovel?)
    };

    private const string NoneSentinel = "NONE.NONE";

    public Run Parse(string rawJson)
    {
        using JsonDocument doc = ParseDocument(rawJson);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidRunFileException("Run file must be a JSON object.");
        }

        string GetString(string name) =>
            root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString()!
                : throw new InvalidRunFileException($"Missing or invalid string field: {name}");

        int GetInt(string name) =>
            root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var value)
                ? value
                : throw new InvalidRunFileException($"Missing or invalid int field: {name}");

        long GetLong(string name) =>
            root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out var value)
                ? value
                : throw new InvalidRunFileException($"Missing or invalid int field: {name}");

        bool GetBool(string name) =>
            root.TryGetProperty(name, out var p) && (p.ValueKind == JsonValueKind.True || p.ValueKind == JsonValueKind.False)
                ? p.GetBoolean()
                : throw new InvalidRunFileException($"Missing or invalid bool field: {name}");

        var run = new Run
        {
            Character = ParseCharacter(root),
            Won = GetBool("win"),
            Ascension = GetInt("ascension"),
            TimeElapsed = GetInt("run_time"),
            TimeStarted = DateTimeOffset.FromUnixTimeSeconds(GetLong("start_time")).UtcDateTime,
            Seed = GetString("seed"),
            BuildId = GetString("build_id"),
            GameMode = GetString("game_mode"),
            KilledByEncounter = NullIfNone(GetString("killed_by_encounter")),
            SchemaVersion = GetInt("schema_version"),
            RunJson = rawJson,
        };

        // map_point_history is an array of acts, each an array of floors.
        // One pass counts floors and extracts the choices on each one.
        if (root.TryGetProperty("map_point_history", out var history) && history.ValueKind == JsonValueKind.Array)
        {
            int totalFloors = 0;
            int actIndex = 0;

            foreach (var act in history.EnumerateArray())
            {
                if (act.ValueKind == JsonValueKind.Array)
                {
                    int floorIndex = 0;
                    foreach (var mapPoint in act.EnumerateArray())
                    {
                        totalFloors++;
                        ExtractCardChoices(mapPoint, actIndex, floorIndex, run.CardChoices);
                        ExtractRelicChoices(mapPoint, actIndex, floorIndex, run.RelicChoices);
                        floorIndex++;
                    }
                }

                actIndex++;
            }

            run.FloorReached = totalFloors;
        }

        return run;
    }

    private static JsonDocument ParseDocument(string rawJson)
    {
        try
        {
            return JsonDocument.Parse(rawJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidRunFileException("File is not valid JSON.", ex);
        }
    }

    private static string? NullIfNone(string value) => value == NoneSentinel ? null : value;

    // The character lives at players[0].character (the format supports multiplayer).
    private static string ParseCharacter(JsonElement root)
    {
        if (!root.TryGetProperty("players", out var players)
            || players.ValueKind != JsonValueKind.Array
            || players.GetArrayLength() == 0)
        {
            throw new InvalidRunFileException("Missing or empty players array.");
        }

        var firstPlayer = players[0];
        if (firstPlayer.ValueKind != JsonValueKind.Object
            || !firstPlayer.TryGetProperty("character", out var character)
            || character.ValueKind != JsonValueKind.String)
        {
            throw new InvalidRunFileException("Missing players[0].character.");
        }

        return character.GetString()!;
    }

    // Reads the floor's Source and the first player's stats, or returns false
    // if the floor is malformed or has no player stats (the caller skips it).
    private static bool TryGetFloorContext(JsonElement mapPoint, out string source, out JsonElement playerStats)
    {
        source = string.Empty;
        playerStats = default;

        if (mapPoint.ValueKind != JsonValueKind.Object
            || !mapPoint.TryGetProperty("map_point_type", out var typeProp)
            || typeProp.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string mapType = typeProp.GetString()!;
        source = SourceByMapPointType.TryGetValue(mapType, out var mapped) ? mapped : mapType;

        if (!mapPoint.TryGetProperty("player_stats", out var stats)
            || stats.ValueKind != JsonValueKind.Array
            || stats.GetArrayLength() == 0
            || stats[0].ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        playerStats = stats[0];
        return true;
    }

    private static bool WasPicked(JsonElement choice) =>
        choice.TryGetProperty("was_picked", out var wasPicked) && wasPicked.ValueKind == JsonValueKind.True;

    // Each entry looks like { "card": { "id": "CARD.X", ... }, "was_picked": true }.
    private static void ExtractCardChoices(JsonElement mapPoint, int actIndex, int floorIndex, List<CardChoice> output)
    {
        if (!TryGetFloorContext(mapPoint, out var source, out var playerStats))
            return;

        if (!playerStats.TryGetProperty("card_choices", out var cardChoices) || cardChoices.ValueKind != JsonValueKind.Array)
            return;

        foreach (var choice in cardChoices.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object
                || !choice.TryGetProperty("card", out var card)
                || card.ValueKind != JsonValueKind.Object
                || !card.TryGetProperty("id", out var id)
                || id.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            output.Add(new CardChoice
            {
                CardId = id.GetString()!,
                Picked = WasPicked(choice),
                Act = actIndex,
                Floor = floorIndex,
                Source = source,
            });
        }
    }

    // Each entry looks like { "choice": "RELIC.X", "was_picked": true }.
    private static void ExtractRelicChoices(JsonElement mapPoint, int actIndex, int floorIndex, List<RelicChoice> output)
    {
        if (!TryGetFloorContext(mapPoint, out var source, out var playerStats))
            return;

        if (!playerStats.TryGetProperty("relic_choices", out var relicChoices) || relicChoices.ValueKind != JsonValueKind.Array)
            return;

        foreach (var choice in relicChoices.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object
                || !choice.TryGetProperty("choice", out var id)
                || id.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            output.Add(new RelicChoice
            {
                RelicId = id.GetString()!,
                Picked = WasPicked(choice),
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
