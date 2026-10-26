using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
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
        var averageFloor = await runs.AverageAsync(r => (double)r.FloorReached);

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

        return Ok(new RunStatsDto
        {
            TotalRuns = totalRuns,
            TotalWins = totalWins,
            TotalLosses = totalRuns - totalWins,
            OverallWinRate = (double)totalWins / totalRuns,
            AverageFloorReached = averageFloor,
            ByCharacter = byCharacter,
            ByAscension = byAscension,
        });
    }
}