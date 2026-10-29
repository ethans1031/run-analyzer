using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Entities;

namespace RunAnalyzer.Api.Data;

public class RunDbContext : DbContext
{
    public RunDbContext(DbContextOptions<RunDbContext> options) : base(options)
    {
    }

    public DbSet<Run> Runs => Set<Run>();
}
