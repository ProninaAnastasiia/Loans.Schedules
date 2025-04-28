using Loans.Schedules.Kafka.Events;

namespace Loans.Schedules.Services;

public interface IRepaymentCalculationService
{
    Task<Guid> CalculateRepaymentAsync(CalculateRepaymentScheduleEvent contractEvent, CancellationToken cancellationToken);
}