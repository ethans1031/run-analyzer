using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Data;
using RunAnalyzer.Api.DTOs;
using RunAnalyzer.Api.Entities;
using RunAnalyzer.Api.Services;

namespace RunAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly RunDbContext _dbContext;
    private readonly RunParser _parser;

    public RunsController(RunDbContext dbContext, RunParser parser)
    {
        _dbContext = dbContext;
        _parser = parser;
    }

    [HttpGet]
    public async Task<ActionResult<List<RunSummaryDto>>> GetRuns(
        [FromQuery] string? card = null,
        [FromQuery] string? relic = null)
    {
        IQueryable<Run> query = _dbContext.Runs;

        if (!string.IsNullOrWhiteSpace(card))
        {
            query = query.Where(r => r.CardChoices.Any(c => c.CardId == card && c.Picked));
        }

        if (!string.IsNullOrWhiteSpace(relic))
        {
            query = query.Where(r => r.RelicChoices.Any(rc => rc.RelicId == relic && rc.Picked));
        }

        var runs = await query
            .OrderByDescending(run => run.Id)
            .Select(run => new RunSummaryDto
            {
                Id = run.Id,
                CharacterName = run.Character,
                Won = run.Won,
                FloorReached = run.FloorReached,
                Ascension = run.Ascension,
                TimeStarted = run.TimeStarted,
                TimeElapsed = run.TimeElapsed,
            })
            .ToListAsync();

        return Ok(runs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RunDetailDto>> GetRun(int id)
    {
        var run = await _dbContext.Runs
            .Where(r => r.Id == id)
            .Select(r => new RunDetailDto
            {
                Id = r.Id,
                CharacterName = r.Character,
                Won = r.Won,
                Ascension = r.Ascension,
                FloorReached = r.FloorReached,
                TimeElapsed = r.TimeElapsed,
                TimeStarted = r.TimeStarted,
                Seed = r.Seed,
                BuildId = r.BuildId,
                GameMode = r.GameMode,
                KilledByEncounter = r.KilledByEncounter,
                SchemaVersion = r.SchemaVersion,
                CardChoices = r.CardChoices
                    .Select(c => new CardChoiceDto
                    {
                        Id = c.Id,
                        CardId = c.CardId,
                        Picked = c.Picked,
                        Act = c.Act,
                        Floor = c.Floor,
                        Source = c.Source,
                    })
                    .ToList(),
                RelicChoices = r.RelicChoices
                    .Select(rc => new RelicChoiceDto
                    {
                        Id = rc.Id,
                        RelicId = rc.RelicId,
                        Picked = rc.Picked,
                        Act = rc.Act,
                        Floor = rc.Floor,
                        Source = rc.Source,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync();

        if (run is null)
        {
            return NotFound();
        }

        return Ok(run);
    }


    [HttpPost("upload")]
    public async Task<ActionResult> UploadRuns([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var results = new List<object>();
        var errors = new List<object>();

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                errors.Add(new { file = file.FileName, error = "File is empty." });
                continue;
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();

            try
            {
                Run run = _parser.Parse(json);

                bool alreadyExists = await _dbContext.Runs.AnyAsync(r => r.Seed == run.Seed);
                if (alreadyExists)
                {
                    errors.Add(new { file = file.FileName, error = "Already uploaded (duplicate seed)." });
                    continue;
                }

                _dbContext.Runs.Add(run);
                await _dbContext.SaveChangesAsync();
                results.Add(new { file = file.FileName, id = run.Id });
            }
            catch (InvalidRunFileException ex)
            {
                errors.Add(new { file = file.FileName, error = ex.Message });
            }
        }

        return Ok(new { uploaded = results, failed = errors });
    }
}
