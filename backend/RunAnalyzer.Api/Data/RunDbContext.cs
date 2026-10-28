using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Entities;

namespace RunAnalyzer.Api.Data;

/// <summary>
/// EF Core session for the run database. CardChoice and RelicChoice are
/// discovered through the navigation properties on <see cref="Run"/>.
/// </summary>
public class RunDbContext : DbContext
{
    public RunDbContext(DbContextOptions<RunDbContext> options) : base(options)
    {
    }

    public DbSet<Run> Runs => Set<Run>();
}
