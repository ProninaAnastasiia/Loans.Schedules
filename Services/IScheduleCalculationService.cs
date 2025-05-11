using Loans.Schedules.Kafka.Events;

namespace Loans.Schedules.Services;

public interface IScheduleCalculationService
{
    Task<Guid> CalculateRepaymentAsync(Guid contractId, decimal loanAmount, int loanTermMonths, decimal interestRate, string paymentType, CancellationToken cancellationToken);
}