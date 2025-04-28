using Loans.Schedules.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Schedules.Data;

public class SchedulesDbContext: DbContext
{
    public SchedulesDbContext(DbContextOptions<SchedulesDbContext> options) : base(options)
    {
    }
    
    public DbSet<ScheduleEntity> Schedules { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduleEntity>().HasKey(u => u.ScheduleId);
        
        base.OnModelCreating(modelBuilder);
    }

}