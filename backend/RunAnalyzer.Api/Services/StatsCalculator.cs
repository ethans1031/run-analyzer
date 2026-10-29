using RunAnalyzer.Api.DTOs;

namespace RunAnalyzer.Api.Services;

public record ChoiceRow(string ItemId, bool Picked, string Source, int RunId, bool Won);

public record RunFact(DateTime TimeStarted, bool Won, int FloorReached);

public static class StatsCalculator
{
    public static readonly IReadOnlySet<string> CardChoiceSources = new HashSet<string> { "combat", "elite", "boss" };

    public static readonly IReadOnlySet<string> RelicChoiceSources = new HashSet<string> { "shop" };

    public const int FloorBucketSize = 5;

    public static List<ChoiceStatsDto> CalculateChoiceStats(
        IEnumerable<ChoiceRow> rows,
        IReadOnlySet<string> choiceSources,
        double baselineWinRate)
    {
        return rows
            .GroupBy(r => r.ItemId)
            .Select(item =>
            {
                var offers = item.Where(r => choiceSources.Contains(r.Source)).ToList();
                int timesOffered = offers.Count;
                int timesPicked = offers.Count(r => r.Picked);

                var pickedRunResults = item
                    .Where(r => r.Picked)
                    .GroupBy(r => r.RunId)
                    .Select(run => run.First().Won)
                    .ToList();
                int runsPicked = pickedRunResults.Count;
                int runsWon = pickedRunResults.Count(won => won);
                double? winRate = runsPicked > 0 ? (double)runsWon / runsPicked : null;

                return new ChoiceStatsDto
                {
                    Id = item.Key,
                    TimesOffered = timesOffered,
                    TimesPicked = timesPicked,
                    PickRate = timesOffered > 0 ? (double)timesPicked / timesOffered : null,
                    RunsPicked = runsPicked,
                    RunsWon = runsWon,
                    WinRate = winRate,
                    WinRateDelta = winRate - baselineWinRate,
                };
            })
            .OrderByDescending(s => s.RunsPicked)
            .ThenByDescending(s => s.TimesOffered)
            .ThenBy(s => s.Id, StringComparer.Ordinal)
            .ToList();
    }

    public static DateOnly WeekStart(DateTime timestamp)
    {
        var date = DateOnly.FromDateTime(timestamp);
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }

    public static List<WeeklyStatsDto> CalculateWeeklyStats(IEnumerable<RunFact> runs)
    {
        return runs
            .GroupBy(r => WeekStart(r.TimeStarted))
            .OrderBy(week => week.Key)
            .Select(week =>
            {
                int runCount = week.Count();
                int winCount = week.Count(r => r.Won);
                return new WeeklyStatsDto
                {
                    WeekStart = week.Key,
                    RunCount = runCount,
                    WinCount = winCount,
                    WinRate = (double)winCount / runCount,
                };
            })
            .ToList();
    }

    public static int LongestWinStreak(IEnumerable<RunFact> runs)
    {
        int longest = 0;
        int current = 0;
        foreach (var run in runs.OrderBy(r => r.TimeStarted))
        {
            current = run.Won ? current + 1 : 0;
            longest = Math.Max(longest, current);
        }
        return longest;
    }

    public static List<FloorBucketDto> CalculateLossesByFloor(IEnumerable<RunFact> runs, int bucketSize = FloorBucketSize)
    {
        var lossCounts = runs
            .Where(r => !r.Won)
            .GroupBy(r => BucketStart(r.FloorReached, bucketSize))
            .ToDictionary(g => g.Key, g => g.Count());

        if (lossCounts.Count == 0)
        {
            return new List<FloorBucketDto>();
        }

        var buckets = new List<FloorBucketDto>();
        for (int start = 1; start <= lossCounts.Keys.Max(); start += bucketSize)
        {
            buckets.Add(new FloorBucketDto
            {
                FloorStart = start,
                FloorEnd = start + bucketSize - 1,
                Losses = lossCounts.GetValueOrDefault(start),
            });
        }

        return buckets;
    }

    private static int BucketStart(int floor, int bucketSize) =>
        (Math.Max(floor, 1) - 1) / bucketSize * bucketSize + 1;
}
