namespace Loans.Schedules.Data.Models;

public class ScheduleEntity
{
    public Guid ScheduleId { get; set; }
    public Guid ContractId { get; set; }
    public DateTime CalculationDate { get; set; } // Дата расчета графика
    public ICollection<ScheduleItemEntity> ScheduleItems { get; set; } = new List<ScheduleItemEntity>();
}