using System.Text.Json.Nodes;
using RunAnalyzer.Api.Services;

namespace RunAnalyzer.Api.Tests;

/// <summary>
/// Unit tests for <see cref="RunParser"/>. Small hand-built JSON documents cover
/// individual rules; the real sample runs in /samples cover the full file format.
/// </summary>
public class RunParserTests
{
    private readonly RunParser _parser = new();

    // ---- Run-level fields --------------------------------------------------

    [Fact]
    public void Parse_MinimalRun_ReadsTopLevelFields()
    {
        var json = MinimalRun();

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal("CHARACTER.IRONCLAD", run.Character);
        Assert.False(run.Won);
        Assert.Equal(5, run.Ascension);
        Assert.Equal(600, run.TimeElapsed);
        Assert.Equal("TESTSEED", run.Seed);
        Assert.Equal("v0.0.0", run.BuildId);
        Assert.Equal("standard", run.GameMode);
        Assert.Equal(9, run.SchemaVersion);
    }

    [Fact]
    public void Parse_ConvertsUnixStartTimeToUtcDateTime()
    {
        var json = MinimalRun();
        json["start_time"] = 1779378420;

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(DateTimeKind.Utc, run.TimeStarted.Kind);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1779378420).UtcDateTime, run.TimeStarted);
    }

    [Fact]
    public void Parse_NoneSentinelKillCause_BecomesNull()
    {
        var json = MinimalRun();
        json["killed_by_encounter"] = "NONE.NONE";

        var run = _parser.Parse(json.ToJsonString());

        Assert.Null(run.KilledByEncounter);
    }

    [Fact]
    public void Parse_RealKillCause_IsKept()
    {
        var json = MinimalRun();
        json["killed_by_encounter"] = "ENCOUNTER.THE_KIN_BOSS";

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal("ENCOUNTER.THE_KIN_BOSS", run.KilledByEncounter);
    }

    [Fact]
    public void Parse_KeepsRawJson()
    {
        string raw = MinimalRun().ToJsonString();

        var run = _parser.Parse(raw);

        Assert.Equal(raw, run.RunJson);
    }

    // ---- Floors and choices ------------------------------------------------

    [Fact]
    public void Parse_FloorReached_CountsFloorsAcrossAllActs()
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(Floor("monster"), Floor("rest_site"), Floor("boss")),
            new JsonArray(Floor("monster"), Floor("shop")));

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(5, run.FloorReached);
    }

    [Fact]
    public void Parse_MissingMapPointHistory_GivesZeroFloorsAndNoChoices()
    {
        var json = MinimalRun();
        json.Remove("map_point_history");

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(0, run.FloorReached);
        Assert.Empty(run.CardChoices);
        Assert.Empty(run.RelicChoices);
    }

    [Fact]
    public void Parse_CardChoices_RecordPickedAndSkippedCardsWithActAndFloor()
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(
                Floor("rest_site"),
                Floor("monster", cards: new JsonArray(
                    Card("CARD.BOOST_AWAY", picked: true),
                    Card("CARD.GUNK_UP", picked: false),
                    Card("CARD.SYNCHRONIZE", picked: false)))));

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(3, run.CardChoices.Count);
        Assert.All(run.CardChoices, c =>
        {
            Assert.Equal(0, c.Act);
            Assert.Equal(1, c.Floor);
            Assert.Equal("combat", c.Source);
        });
        var picked = Assert.Single(run.CardChoices, c => c.Picked);
        Assert.Equal("CARD.BOOST_AWAY", picked.CardId);
    }

    [Fact]
    public void Parse_RelicChoices_RecordPickedAndSkippedRelics()
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(),
            new JsonArray(
                Floor("shop", relics: new JsonArray(
                    Relic("RELIC.STRAWBERRY", picked: true),
                    Relic("RELIC.MINIATURE_CANNON", picked: false)))));

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(2, run.RelicChoices.Count);
        Assert.All(run.RelicChoices, r =>
        {
            Assert.Equal(1, r.Act);
            Assert.Equal(0, r.Floor);
            Assert.Equal("shop", r.Source);
        });
        Assert.True(run.RelicChoices.Single(r => r.RelicId == "RELIC.STRAWBERRY").Picked);
        Assert.False(run.RelicChoices.Single(r => r.RelicId == "RELIC.MINIATURE_CANNON").Picked);
    }

    [Theory]
    [InlineData("monster", "combat")]
    [InlineData("elite", "elite")]
    [InlineData("boss", "boss")]
    [InlineData("shop", "shop")]
    [InlineData("treasure", "treasure")]
    [InlineData("ancient", "ancient")]
    [InlineData("unknown", "event")]
    [InlineData("some_future_type", "some_future_type")]
    public void Parse_MapPointType_SetsChoiceSource(string mapPointType, string expectedSource)
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(Floor(mapPointType,
                cards: new JsonArray(Card("CARD.STRIKE", picked: true)),
                relics: new JsonArray(Relic("RELIC.ANCHOR", picked: true)))));

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal(expectedSource, Assert.Single(run.CardChoices).Source);
        Assert.Equal(expectedSource, Assert.Single(run.RelicChoices).Source);
    }

    [Fact]
    public void Parse_MalformedChoiceEntries_AreSkippedNotFatal()
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(
                Floor("monster", cards: new JsonArray(
                    Card("CARD.GOOD", picked: true),
                    new JsonObject { ["was_picked"] = true },                        // no card
                    new JsonObject { ["card"] = new JsonObject { ["id"] = 42 } },    // id not a string
                    "not an object")),
                new JsonObject { ["player_stats"] = new JsonArray() },               // no map_point_type
                "not a floor"));

        var run = _parser.Parse(json.ToJsonString());

        Assert.Equal("CARD.GOOD", Assert.Single(run.CardChoices).CardId);
        Assert.Equal(3, run.FloorReached);
    }

    [Fact]
    public void Parse_MissingWasPicked_TreatedAsSkipped()
    {
        var json = MinimalRun();
        json["map_point_history"] = new JsonArray(
            new JsonArray(Floor("monster", cards: new JsonArray(
                new JsonObject { ["card"] = new JsonObject { ["id"] = "CARD.X" } }))));

        var run = _parser.Parse(json.ToJsonString());

        Assert.False(Assert.Single(run.CardChoices).Picked);
    }

    // ---- Invalid files -----------------------------------------------------

    [Fact]
    public void Parse_InvalidJson_ThrowsInvalidRunFile()
    {
        var ex = Assert.Throws<InvalidRunFileException>(() => _parser.Parse("{ this is not json"));
        Assert.Equal("File is not valid JSON.", ex.Message);
    }

    [Fact]
    public void Parse_JsonThatIsNotAnObject_ThrowsInvalidRunFile()
    {
        Assert.Throws<InvalidRunFileException>(() => _parser.Parse("[1, 2, 3]"));
    }

    [Theory]
    [InlineData("win")]
    [InlineData("ascension")]
    [InlineData("run_time")]
    [InlineData("start_time")]
    [InlineData("seed")]
    [InlineData("build_id")]
    [InlineData("game_mode")]
    [InlineData("killed_by_encounter")]
    [InlineData("schema_version")]
    [InlineData("players")]
    public void Parse_MissingRequiredField_ThrowsInvalidRunFile(string field)
    {
        var json = MinimalRun();
        json.Remove(field);

        Assert.Throws<InvalidRunFileException>(() => _parser.Parse(json.ToJsonString()));
    }

    [Fact]
    public void Parse_WrongFieldType_ThrowsInvalidRunFile()
    {
        var json = MinimalRun();
        json["win"] = "yes";

        Assert.Throws<InvalidRunFileException>(() => _parser.Parse(json.ToJsonString()));
    }

    [Fact]
    public void Parse_EmptyPlayers_ThrowsInvalidRunFile()
    {
        var json = MinimalRun();
        json["players"] = new JsonArray();

        Assert.Throws<InvalidRunFileException>(() => _parser.Parse(json.ToJsonString()));
    }

    // ---- Real sample files -------------------------------------------------

    [Fact]
    public void Parse_SampleWinningRun_MatchesKnownValues()
    {
        var run = _parser.Parse(ReadSample("1779464273.run"));

        Assert.Equal("CHARACTER.DEFECT", run.Character);
        Assert.True(run.Won);
        Assert.Equal(10, run.Ascension);
        Assert.Equal("C7DA4HH4QT", run.Seed);
        Assert.Equal("v0.106.0", run.BuildId);
        Assert.Null(run.KilledByEncounter);
        Assert.Equal(49, run.FloorReached);

        Assert.Equal(105, run.CardChoices.Count);
        Assert.Equal(23, run.CardChoices.Count(c => c.Picked));
        Assert.Equal(27, run.RelicChoices.Count);
        Assert.Equal(18, run.RelicChoices.Count(r => r.Picked));

        var firstCard = run.CardChoices[0];
        Assert.Equal(("CARD.THINKING_AHEAD", 0, 0, "ancient", false),
            (firstCard.CardId, firstCard.Act, firstCard.Floor, firstCard.Source, firstCard.Picked));
    }

    [Fact]
    public void Parse_SampleLosingRun_KeepsKillCause()
    {
        var run = _parser.Parse(ReadSample("1779378420.run"));

        Assert.False(run.Won);
        Assert.Equal("ENCOUNTER.THE_KIN_BOSS", run.KilledByEncounter);
        Assert.Equal(17, run.FloorReached);
        Assert.Equal(27, run.CardChoices.Count);
        Assert.Equal(9, run.RelicChoices.Count);
    }

    [Fact]
    public void Parse_AllSampleRuns_ProduceChoicesWithKnownSources()
    {
        var knownSources = new[] { "combat", "elite", "boss", "shop", "treasure", "ancient", "event" };
        string[] files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "SampleRuns"), "*.run");
        Assert.NotEmpty(files);

        foreach (string file in files)
        {
            var run = _parser.Parse(File.ReadAllText(file));

            Assert.NotEmpty(run.CardChoices);
            Assert.All(run.CardChoices, c => Assert.Contains(c.Source, knownSources));
            Assert.All(run.RelicChoices, r => Assert.Contains(r.Source, knownSources));
        }
    }

    // ---- Helpers -----------------------------------------------------------

    private static string ReadSample(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SampleRuns", fileName));

    private static JsonObject MinimalRun() => new()
    {
        ["win"] = false,
        ["ascension"] = 5,
        ["run_time"] = 600,
        ["start_time"] = 1779378420,
        ["seed"] = "TESTSEED",
        ["build_id"] = "v0.0.0",
        ["game_mode"] = "standard",
        ["killed_by_encounter"] = "NONE.NONE",
        ["schema_version"] = 9,
        ["players"] = new JsonArray(new JsonObject { ["character"] = "CHARACTER.IRONCLAD" }),
        ["map_point_history"] = new JsonArray(),
    };

    private static JsonObject Floor(string mapPointType, JsonArray? cards = null, JsonArray? relics = null)
    {
        var stats = new JsonObject();
        if (cards is not null)
        {
            stats["card_choices"] = cards;
        }
        if (relics is not null)
        {
            stats["relic_choices"] = relics;
        }

        return new JsonObject
        {
            ["map_point_type"] = mapPointType,
            ["player_stats"] = new JsonArray(stats),
        };
    }

    private static JsonObject Card(string id, bool picked) => new()
    {
        ["card"] = new JsonObject { ["id"] = id },
        ["was_picked"] = picked,
    };

    private static JsonObject Relic(string id, bool picked) => new()
    {
        ["choice"] = id,
        ["was_picked"] = picked,
    };
}
