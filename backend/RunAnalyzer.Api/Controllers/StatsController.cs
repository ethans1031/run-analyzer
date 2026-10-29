using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Data;
using RunAnalyzer.Api.DTOs;
using RunAnalyzer.Api.Entities;
using RunAnalyzer.Api.Services;

namespace RunAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private const int TopKillerCount = 10;

    private readonly RunDbContext _dbContext;

    public StatsController(RunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<RunStatsDto>> GetStats()
    {
        var runs = _dbContext.Runs;

        var totalRuns = await runs.CountAsync();

        if (totalRuns == 0)
        {
            return Ok(new RunStatsDto());
        }

        var totalWins = await runs.CountAsync(r => r.Won);
        var totalLosses = totalRuns - totalWins;
        var averageFloor = await runs.AverageAsync(r => (double)r.FloorReached);

        var averageWinTime = await runs.Where(r => r.Won).Select(r => (double?)r.TimeElapsed).AverageAsync();
        var averageLossTime = await runs.Where(r => !r.Won).Select(r => (double?)r.TimeElapsed).AverageAsync();

        var byCharacter = await runs.GroupBy(r => r.Character).Select(g => new CharacterStatsDto
        {
            Character = g.Key,
            RunCount = g.Count(),
            WinCount = g.Sum(r => r.Won ? 1 : 0),
            WinRate = (double)g.Sum(r => r.Won ? 1 : 0) / g.Count(),
            AverageFloorReached = g.Average(r => (double)r.FloorReached),
        }).OrderByDescending(c => c.RunCount).ToListAsync();

        var byAscension = await runs.GroupBy(r => r.Ascension).Select(g => new AscensionStatsDto
        {
            Ascension = g.Key,
            RunCount = g.Count(),
            WinCount = g.Sum(r => r.Won ? 1 : 0),
            WinRate = (double)g.Sum(r => r.Won ? 1 : 0) / g.Count(),
        }).OrderBy(a => a.Ascension).ToListAsync();

        var killerCounts = await runs
            .Where(r => !r.Won && r.KilledByEncounter != null)
            .GroupBy(r => r.KilledByEncounter!)
            .Select(g => new { Encounter = g.Key, Count = g.Count() })
            .OrderByDescending(k => k.Count)
            .Take(TopKillerCount)
            .ToListAsync();

        var topKillers = killerCounts.Select(k => new KillerStatsDto
        {
            Encounter = k.Encounter,
            Count = k.Count,
            ShareOfLosses = totalLosses > 0 ? (double)k.Count / totalLosses : 0,
        }).ToList();


        var runFacts = await runs
            .Select(r => new RunFact(r.TimeStarted, r.Won, r.FloorReached))
            .ToListAsync();

        return Ok(new RunStatsDto
        {
            TotalRuns = totalRuns,
            TotalWins = totalWins,
            TotalLosses = totalLosses,
            OverallWinRate = (double)totalWins / totalRuns,
            AverageFloorReached = averageFloor,
            AverageWinTime = averageWinTime,
            AverageLossTime = averageLossTime,
            LongestWinStreak = StatsCalculator.LongestWinStreak(runFacts),
            ByCharacter = byCharacter,
            ByAscension = byAscension,
            TopKillers = topKillers,
            LossesByFloor = StatsCalculator.CalculateLossesByFloor(runFacts),
            ByWeek = StatsCalculator.CalculateWeeklyStats(runFacts),
        });
    }


    [HttpGet("cards")]
    public async Task<ActionResult<ChoiceStatsResponseDto>> GetCardStats([FromQuery] string? character = null)
    {
        var runs = FilterByCharacter(character);

        // One flat row per card offered, joined to its run's result.
        var rows = await _dbContext.Set<CardChoice>()
            .Join(runs, c => c.RunId, r => r.Id, (c, r) => new { c.CardId, c.Picked, c.Source, RunId = r.Id, r.Won })
            .ToListAsync();

        var choiceRows = rows.Select(x => new ChoiceRow(x.CardId, x.Picked, x.Source, x.RunId, x.Won));

        return Ok(await BuildChoiceStatsAsync(runs, choiceRows, StatsCalculator.CardChoiceSources));
    }


    [HttpGet("relics")]
    public async Task<ActionResult<ChoiceStatsResponseDto>> GetRelicStats([FromQuery] string? character = null)
    {
        var runs = FilterByCharacter(character);

        // One flat row per relic offered, joined to its run's result.
        var rows = await _dbContext.Set<RelicChoice>()
            .Join(runs, c => c.RunId, r => r.Id, (c, r) => new { c.RelicId, c.Picked, c.Source, RunId = r.Id, r.Won })
            .ToListAsync();

        var choiceRows = rows.Select(x => new ChoiceRow(x.RelicId, x.Picked, x.Source, x.RunId, x.Won));

        return Ok(await BuildChoiceStatsAsync(runs, choiceRows, StatsCalculator.RelicChoiceSources));
    }

    private IQueryable<Run> FilterByCharacter(string? character)
    {
        IQueryable<Run> runs = _dbContext.Runs;
        if (!string.IsNullOrWhiteSpace(character))
        {
            runs = runs.Where(r => r.Character == character);
        }
        return runs;
    }

    private static async Task<ChoiceStatsResponseDto> BuildChoiceStatsAsync(
        IQueryable<Run> runs,
        IEnumerable<ChoiceRow> choiceRows,
        IReadOnlySet<string> choiceSources)
    {
        int runCount = await runs.CountAsync();
        int winCount = await runs.CountAsync(r => r.Won);
        double baseline = runCount > 0 ? (double)winCount / runCount : 0;

        return new ChoiceStatsResponseDto
        {
            RunCount = runCount,
            BaselineWinRate = baseline,
            Items = StatsCalculator.CalculateChoiceStats(choiceRows, choiceSources, baseline),
        };
    }
}
