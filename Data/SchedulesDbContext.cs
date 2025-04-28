using Loans.Schedules.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Schedules.Data;

public class SchedulesDbContext: DbContext
{
    public SchedulesDbContext(DbContextOptions<SchedulesDbContext> options) : base(options)
    {
    }
    
    public DbSet<ScheduleEntity> Schedules { get; set; }
    public DbSet<ScheduleItemEntity> ScheduleItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduleEntity>().HasKey(u => u.ScheduleId);
        modelBuilder.Entity<ScheduleItemEntity>().HasKey(u => u.ScheduleItemId);

        modelBuilder.Entity<ScheduleEntity>()
            .HasMany(s => s.ScheduleItems)
            .WithOne(si => si.Schedule)
            .HasForeignKey(si => si.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade); // При удалении графика удаляются и связанные платежи
        
        base.OnModelCreating(modelBuilder);
    }

}