//DbContext for entityframework, represents a database connection
using Microsoft.EntityFrameworkCore;
using backend.Entities;

namespace backend.Data;

public class RunDbContext : DbContext
{

    public DbSet<Run> Runs => Set<Run>();
    public RunDbContext (DbContextOptions<RunDbContext> options) : base(options)
    {
        
    }


}