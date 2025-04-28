using Loans.Schedules.Data.Models;

namespace Loans.Schedules.Data.Repositories;

public interface IScheduleRepository
{
    Task<ScheduleEntity?> GetByIdAsync(Guid scheduleId);
    Task SaveAsync(ScheduleEntity scheduleId);
    Task UpdateAsync(ScheduleEntity scheduleId);
}