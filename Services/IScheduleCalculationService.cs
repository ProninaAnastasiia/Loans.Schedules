using Loans.Schedules.Kafka.Events;

namespace Loans.Schedules.Services;

public interface IScheduleCalculationService
{
    Task<Guid> CalculateRepaymentAsync(CalculateContractValuesEvent contractEvent, CancellationToken cancellationToken);
}