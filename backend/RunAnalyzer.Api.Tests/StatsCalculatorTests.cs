using RunAnalyzer.Api.Services;

namespace RunAnalyzer.Api.Tests;

public class StatsCalculatorTests
{
    private static readonly IReadOnlySet<string> CardSources = StatsCalculator.CardChoiceSources;

    // ---- Card and relic stats ----------------------------------------------

    [Fact]
    public void ChoiceStats_PickRate_OnlyCountsChoiceSources()
    {
        var rows = new[]
        {
            new ChoiceRow("CARD.A", Picked: true, Source: "combat", RunId: 1, Won: true),
            new ChoiceRow("CARD.A", Picked: false, Source: "elite", RunId: 1, Won: true),
            new ChoiceRow("CARD.A", Picked: false, Source: "shop", RunId: 2, Won: false),
            new ChoiceRow("CARD.A", Picked: true, Source: "event", RunId: 3, Won: false),
        };

        var stats = Assert.Single(StatsCalculator.CalculateChoiceStats(rows, CardSources, baselineWinRate: 0.5));

        Assert.Equal(2, stats.TimesOffered);
        Assert.Equal(1, stats.TimesPicked);
        Assert.Equal(0.5, stats.PickRate);
    }

    [Fact]
    public void ChoiceStats_WinRate_CountsEachRunOnceAndIncludesAllSources()
    {
        var rows = new[]
        {
            // Picked twice in a winning run: still one run.
            new ChoiceRow("CARD.A", true, "combat", 1, true),
            new ChoiceRow("CARD.A", true, "elite", 1, true),
            // Picked from an event in a losing run: counts toward win rate.
            new ChoiceRow("CARD.A", true, "event", 2, false),
            // Only skipped in this run: doesn't count.
            new ChoiceRow("CARD.A", false, "combat", 3, true),
        };

        var stats = Assert.Single(StatsCalculator.CalculateChoiceStats(rows, CardSources, baselineWinRate: 0.25));

        Assert.Equal(2, stats.RunsPicked);
        Assert.Equal(1, stats.RunsWon);
        Assert.Equal(0.5, stats.WinRate);
        Assert.Equal(0.25, stats.WinRateDelta);
    }

    [Fact]
    public void ChoiceStats_NeverPicked_HasNullWinRate()
    {
        var rows = new[]
        {
            new ChoiceRow("CARD.SKIPPED", false, "combat", 1, true),
            new ChoiceRow("CARD.SKIPPED", false, "boss", 2, false),
        };

        var stats = Assert.Single(StatsCalculator.CalculateChoiceStats(rows, CardSources, 0.5));

        Assert.Equal(0, stats.RunsPicked);
        Assert.Equal(0.0, stats.PickRate);
        Assert.Null(stats.WinRate);
        Assert.Null(stats.WinRateDelta);
    }

    [Fact]
    public void ChoiceStats_NeverOfferedAsChoice_HasNullPickRate()
    {
        var rows = new[] { new ChoiceRow("RELIC.ELITE_DROP", true, "elite", 1, true) };

        var stats = Assert.Single(
            StatsCalculator.CalculateChoiceStats(rows, StatsCalculator.RelicChoiceSources, 0.5));

        Assert.Equal(0, stats.TimesOffered);
        Assert.Null(stats.PickRate);
        Assert.Equal(1, stats.RunsPicked);
    }

    [Fact]
    public void ChoiceStats_SortedByRunsPickedThenOffersThenId()
    {
        var rows = new[]
        {
            new ChoiceRow("CARD.B", false, "combat", 1, true),
            new ChoiceRow("CARD.C", true, "combat", 1, true),
            new ChoiceRow("CARD.C", true, "combat", 2, true),
            new ChoiceRow("CARD.A", false, "combat", 1, true),
            new ChoiceRow("CARD.D", true, "combat", 1, true),
        };

        var ids = StatsCalculator.CalculateChoiceStats(rows, CardSources, 0.5).Select(s => s.Id);

        Assert.Equal(new[] { "CARD.C", "CARD.D", "CARD.A", "CARD.B" }, ids);
    }

    // ---- Weekly trend ------------------------------------------------------

    [Theory]
    [InlineData("2026-10-26", "2026-10-26")] // Monday
    [InlineData("2026-10-25", "2026-10-19")] // Sunday belongs to the previous week
    [InlineData("2026-10-21", "2026-10-19")] // Wednesday
    public void WeekStart_ReturnsMonday(string date, string expectedMonday)
    {
        var result = StatsCalculator.WeekStart(DateTime.Parse(date).AddHours(15));

        Assert.Equal(DateOnly.Parse(expectedMonday), result);
    }

    [Fact]
    public void WeeklyStats_GroupsByWeekInOrder()
    {
        var runs = new[]
        {
            new RunFact(new DateTime(2026, 10, 27), Won: true, FloorReached: 49),
            new RunFact(new DateTime(2026, 10, 20), Won: true, FloorReached: 49),
            new RunFact(new DateTime(2026, 10, 21), Won: false, FloorReached: 12),
        };

        var weeks = StatsCalculator.CalculateWeeklyStats(runs);

        Assert.Equal(2, weeks.Count);
        Assert.Equal(new DateOnly(2026, 10, 19), weeks[0].WeekStart);
        Assert.Equal(2, weeks[0].RunCount);
        Assert.Equal(0.5, weeks[0].WinRate);
        Assert.Equal(new DateOnly(2026, 10, 26), weeks[1].WeekStart);
        Assert.Equal(1.0, weeks[1].WinRate);
    }

    // ---- Win streak --------------------------------------------------------

    [Fact]
    public void LongestWinStreak_UsesStartTimeOrder()
    {
        var day = new DateTime(2026, 10, 1);
        var runs = new[]
        {
            // Listed out of order on purpose.
            new RunFact(day.AddDays(3), Won: true, FloorReached: 49),
            new RunFact(day.AddDays(0), Won: true, FloorReached: 49),
            new RunFact(day.AddDays(1), Won: false, FloorReached: 10),
            new RunFact(day.AddDays(2), Won: true, FloorReached: 49),
            new RunFact(day.AddDays(4), Won: true, FloorReached: 49),
        };

        Assert.Equal(3, StatsCalculator.LongestWinStreak(runs));
    }

    [Fact]
    public void LongestWinStreak_NoWins_IsZero()
    {
        Assert.Equal(0, StatsCalculator.LongestWinStreak(new[] { new RunFact(DateTime.UtcNow, false, 5) }));
    }

    // ---- Where runs end ----------------------------------------------------

    [Fact]
    public void LossesByFloor_BucketsLossesAndFillsGaps()
    {
        var runs = new[]
        {
            new RunFact(DateTime.UtcNow, Won: false, FloorReached: 3),
            new RunFact(DateTime.UtcNow, Won: false, FloorReached: 5),
            new RunFact(DateTime.UtcNow, Won: false, FloorReached: 17),
            new RunFact(DateTime.UtcNow, Won: true, FloorReached: 49),  // wins are ignored
        };

        var buckets = StatsCalculator.CalculateLossesByFloor(runs, bucketSize: 5);

        Assert.Equal(new[] { 1, 6, 11, 16 }, buckets.Select(b => b.FloorStart));
        Assert.Equal(new[] { 5, 10, 15, 20 }, buckets.Select(b => b.FloorEnd));
        Assert.Equal(new[] { 2, 0, 0, 1 }, buckets.Select(b => b.Losses));
    }

    [Fact]
    public void LossesByFloor_NoLosses_ReturnsEmpty()
    {
        var runs = new[] { new RunFact(DateTime.UtcNow, Won: true, FloorReached: 49) };

        Assert.Empty(StatsCalculator.CalculateLossesByFloor(runs));
    }
}
