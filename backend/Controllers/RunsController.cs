using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Services;
using backend.DTOs;


using System;
using System.IO;
using SQLitePCL;

namespace backend.Controllers;

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
    public async Task<ActionResult<List<RunSummaryDto>>> GetRuns()
    {
        var runs = await _dbContext.Runs
        .OrderByDescending(run => run.Id)
        .Select(run => new RunSummaryDto
        {
            Id = run.Id,
            CharacterName = run.Character,
            Won = run.Won,
            FloorReached = run.FloorReached,
            Ascension = run.Ascension,
            TimeStarted = run.TimeStarted,
            TimeElapsed = run.TimeElapsed
        })
        .ToListAsync();

        return Ok(runs);
    }

    [HttpGet]
    [Route("{id}")]
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

    //TEMP TEST FOR POST

    [HttpPost]
    //hardcoded run for now
    public async Task<ActionResult<Run>> CreateRun()
    {
        var run = new Run
        {
            Character = "Silent",
            FloorReached = 20,
            Won = false,
            Ascension = 10,
            RunJson = "{\"character\":\"Silent\",\"floor_reached\":20,\"victory\":false,\"score\":525}",
            TimeStarted = DateTime.UtcNow,
            TimeElapsed = 1000,

        };

        _dbContext.Runs.Add(run);

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRun), new { id = run.Id }, run);
    }

    //TEMP TEST FOR PARSER

    [HttpGet]
    [Route("parse-test")]
    public ActionResult<Run> GetParsedRun()
    {
        string path = @"C:\Users\Ethan\AppData\Roaming\SlayTheSpire2\steam\76561198172300663\profile1\saves\history\1780157310.run";

        string jsonString = System.IO.File.ReadAllText(path);

        Run run = _parser.Parse(jsonString);

        run.RunJson = "[omitted]";

        return Ok(run);


    }

    [HttpPost]
    [Route("upload")]
    public async Task<ActionResult> UploadRuns([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files uploaded");
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

            using var stream = file.OpenReadStream();       //stream object
            using var reader = new StreamReader(stream);          //StreamReader
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