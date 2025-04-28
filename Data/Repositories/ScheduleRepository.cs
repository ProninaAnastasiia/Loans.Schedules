using Loans.Schedules.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Schedules.Data.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly SchedulesDbContext _dbContext;
    private readonly ILogger<ScheduleRepository> _logger;

    public ScheduleRepository(SchedulesDbContext dbContext, ILogger<ScheduleRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<ScheduleEntity?> GetByIdAsync(Guid scheduleId)
    {
        return await _dbContext.Schedules.FirstOrDefaultAsync(p => p.ScheduleId == scheduleId);
    }

    public async Task SaveAsync(ScheduleEntity schedule)
    {
        try
        {
            var existingSchedule =
                await _dbContext.Schedules.FirstOrDefaultAsync(p => p.ScheduleId == schedule.ScheduleId);

            if (existingSchedule == null)
            {
                _dbContext.Schedules.Add(schedule);
            }
            else
            {
                _dbContext.Entry(existingSchedule).CurrentValues.SetValues(schedule);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении графика платежей.");
            throw;
        }
    }

    public async Task UpdateAsync(ScheduleEntity schedule)
    {
        try
        {
            _dbContext.Schedules.Update(schedule);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении графика платежей.");
            throw;
        }
    }
}