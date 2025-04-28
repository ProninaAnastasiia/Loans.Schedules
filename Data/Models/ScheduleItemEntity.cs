namespace Loans.Schedules.Data.Models;

public class ScheduleItemEntity
{
    public Guid ScheduleItemId { get; set; }
    public int PaymentNumber { get; set; } // Номер платежа
    public DateTime PaymentDate { get; set; }   // Дата платежа
    public decimal PaymentAmount { get; set; }  // Общая сумма платежа
    public decimal PrincipalAmount { get; set; } // Сумма погашения основного долга
    public decimal InterestAmount { get; set; } // Сумма погашения процентов
    public decimal RemainingPrincipal { get; set; } // Остаток основного долга после платежа

    // Навигация
    public Guid ScheduleId { get; set; }
    public ScheduleEntity Schedule { get; set; }
}
