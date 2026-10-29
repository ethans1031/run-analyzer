using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Controllers;
using RunAnalyzer.Api.Data;
using RunAnalyzer.Api.DTOs;
using RunAnalyzer.Api.Entities;
using RunAnalyzer.Api.Services;

namespace RunAnalyzer.Api.Tests;

public class ControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly RunDbContext _db;

    public ControllerTests()
    {
        // The in-memory database lives as long as this open connection.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RunDbContext>().UseSqlite(_connection).Options;
        _db = new RunDbContext(options);
        _db.Database.EnsureCreated();

        _db.Runs.AddRange(
            NewRun("SEED1", "CHARACTER.DEFECT", won: true, floor: 49, started: new DateTime(2026, 10, 19, 12, 0, 0),
                cards: new[] { Card("CARD.GAMMA_BLAST", true, "combat"), Card("CARD.GAMMA_BLAST", true, "elite"), Card("CARD.ZAP", false, "combat") },
                relics: new[] { Relic("RELIC.ANCHOR", true, "shop") }),
            NewRun("SEED2", "CHARACTER.DEFECT", won: false, floor: 12, started: new DateTime(2026, 10, 20, 12, 0, 0),
                killedBy: "ENCOUNTER.THE_KIN_BOSS",
                cards: new[] { Card("CARD.GAMMA_BLAST", false, "combat"), Card("CARD.ZAP", true, "combat") },
                relics: new[] { Relic("RELIC.ANCHOR", false, "shop") }),
            NewRun("SEED3", "CHARACTER.REGENT", won: false, floor: 3, started: new DateTime(2026, 10, 27, 12, 0, 0),
                killedBy: "ENCOUNTER.THE_KIN_BOSS",
                cards: new[] { Card("CARD.GAMMA_BLAST", true, "event") },
                relics: Array.Empty<RelicChoice>()));
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ---- Runs filter -------------------------------------------------------

    [Fact]
    public async Task GetRuns_NoFilter_ReturnsAllNewestFirst()
    {
        var runs = await GetRunsAsync(card: null, relic: null);

        Assert.Equal(3, runs.Count);
        Assert.True(runs[0].Id > runs[1].Id);
    }

    [Fact]
    public async Task GetRuns_FilterByCard_ReturnsRunsWhereItWasPicked()
    {
        var runs = await GetRunsAsync(card: "CARD.GAMMA_BLAST", relic: null);

        // SEED2 only skipped it, and SEED1 picked it twice but appears once.
        Assert.Equal(2, runs.Count);
        Assert.Equal(new[] { "CHARACTER.DEFECT", "CHARACTER.REGENT" }, runs.Select(r => r.CharacterName).OrderBy(c => c));
    }

    [Fact]
    public async Task GetRuns_FilterByCardAndRelic_RequiresBoth()
    {
        var runs = await GetRunsAsync(card: "CARD.GAMMA_BLAST", relic: "RELIC.ANCHOR");

        var run = Assert.Single(runs);
        Assert.True(run.Won);
    }

    [Fact]
    public async Task GetRuns_UnknownCard_ReturnsEmpty()
    {
        Assert.Empty(await GetRunsAsync(card: "CARD.DOES_NOT_EXIST", relic: null));
    }

    // ---- Card and relic stats ----------------------------------------------

    [Fact]
    public async Task GetCardStats_ComputesPickRateAndWinRate()
    {
        var response = await GetOkValue(new StatsController(_db).GetCardStats());

        Assert.Equal(3, response.RunCount);
        Assert.Equal(1.0 / 3, response.BaselineWinRate, precision: 6);

        var gamma = response.Items.Single(i => i.Id == "CARD.GAMMA_BLAST");
        Assert.Equal(3, gamma.TimesOffered);   // combat, elite, combat (the event offer isn't a reward screen)
        Assert.Equal(2, gamma.TimesPicked);
        Assert.Equal(2, gamma.RunsPicked);     // SEED1 and SEED3
        Assert.Equal(1, gamma.RunsWon);
        Assert.Equal(0.5, gamma.WinRate);
    }

    [Fact]
    public async Task GetCardStats_FilterByCharacter_LimitsRunsAndBaseline()
    {
        var response = await GetOkValue(new StatsController(_db).GetCardStats("CHARACTER.DEFECT"));

        Assert.Equal(2, response.RunCount);
        Assert.Equal(0.5, response.BaselineWinRate);
        var gamma = response.Items.Single(i => i.Id == "CARD.GAMMA_BLAST");
        Assert.Equal(1, gamma.RunsPicked);
        Assert.Equal(0.5, gamma.WinRateDelta);
    }

    [Fact]
    public async Task GetRelicStats_UsesShopOffersForBuyRate()
    {
        var response = await GetOkValue(new StatsController(_db).GetRelicStats());

        var anchor = Assert.Single(response.Items);
        Assert.Equal("RELIC.ANCHOR", anchor.Id);
        Assert.Equal(2, anchor.TimesOffered);
        Assert.Equal(0.5, anchor.PickRate);
        Assert.Equal(1.0, anchor.WinRate);
    }

    // ---- Overall stats -----------------------------------------------------

    [Fact]
    public async Task GetStats_IncludesKillersFloorsWeeksAndTimes()
    {
        var stats = await GetOkValue(new StatsController(_db).GetStats());

        var killer = Assert.Single(stats.TopKillers);
        Assert.Equal("ENCOUNTER.THE_KIN_BOSS", killer.Encounter);
        Assert.Equal(2, killer.Count);
        Assert.Equal(1.0, killer.ShareOfLosses);

        Assert.Equal(new[] { 1, 0, 1 }, stats.LossesByFloor.Select(b => b.Losses));   // floors 1-5, 6-10, 11-15

        Assert.Equal(2, stats.ByWeek.Count);
        Assert.Equal(new DateOnly(2026, 10, 19), stats.ByWeek[0].WeekStart);

        Assert.Equal(1, stats.LongestWinStreak);
        Assert.Equal(1000.0, stats.AverageWinTime);
        Assert.Equal(1000.0, stats.AverageLossTime);
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task<List<RunSummaryDto>> GetRunsAsync(string? card, string? relic)
    {
        var controller = new RunsController(_db, new RunParser());
        return await GetOkValue(controller.GetRuns(card, relic));
    }

    private static async Task<T> GetOkValue<T>(Task<ActionResult<T>> action)
    {
        var result = await action;
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsAssignableFrom<T>(ok.Value);
    }

    private static Run NewRun(
        string seed, string character, bool won, int floor, DateTime started,
        CardChoice[] cards, RelicChoice[] relics, string? killedBy = null) => new()
    {
        Seed = seed,
        Character = character,
        Won = won,
        FloorReached = floor,
        TimeStarted = started,
        TimeElapsed = 1000,
        Ascension = 10,
        BuildId = "v0.106.1",
        GameMode = "standard",
        KilledByEncounter = killedBy,
        SchemaVersion = 9,
        RunJson = "{}",
        CardChoices = cards.ToList(),
        RelicChoices = relics.ToList(),
    };

    private static CardChoice Card(string id, bool picked, string source) =>
        new() { CardId = id, Picked = picked, Source = source };

    private static RelicChoice Relic(string id, bool picked, string source) =>
        new() { RelicId = id, Picked = picked, Source = source };
}
