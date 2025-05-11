using Loans.Schedules.Data.Models;
using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka.Events;

namespace Loans.Schedules.Services;

public class ScheduleCalculationService : IScheduleCalculationService
{
    private readonly IScheduleRepository _repository;
    private readonly ILogger<ScheduleCalculationService> _logger;

    public ScheduleCalculationService(IScheduleRepository repository, ILogger<ScheduleCalculationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CalculateRepaymentAsync(Guid contractId, decimal loanAmount, int loanTermMonths, decimal interestRate, string paymentType, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(10); // Можно потом заменить на дату из события

        var schedule = new ScheduleEntity
        {
            ScheduleId = Guid.NewGuid(),
            CalculationDate = DateTime.UtcNow,
            ContractId = contractId
        };

        var rate = interestRate / 100m / 12; // месячная ставка
        var months = loanTermMonths;

        if (paymentType.Equals("аннуитет", StringComparison.OrdinalIgnoreCase))
        {
            var annuityFactor = (decimal)(
                (double)rate * Math.Pow(1 + (double)rate, months) /
                (Math.Pow(1 + (double)rate, months) - 1)
            );
            var monthlyPayment = Math.Round(loanAmount * annuityFactor, 2);

            var remaining = loanAmount;

            for (int i = 1; i <= months; i++)
            {
                var interest = Math.Round(remaining * rate, 2);
                var principal = Math.Round(monthlyPayment - interest, 2);
                remaining -= principal;

                schedule.ScheduleItems.Add(new ScheduleItemEntity
                {
                    PaymentNumber = i,
                    PaymentDate = startDate.AddMonths(i),
                    PaymentAmount = monthlyPayment,
                    InterestAmount = interest,
                    PrincipalAmount = principal,
                    RemainingPrincipal = remaining
                });
            }
        }
        else if (paymentType.Equals("дифференцированная", StringComparison.OrdinalIgnoreCase))
        {
            var principalPart = Math.Round(loanAmount / months, 2);
            var remaining = loanAmount;

            for (int i = 1; i <= months; i++)
            {
                var interest = Math.Round(remaining * rate, 2);
                var monthlyPayment = Math.Round(principalPart + interest, 2);
                remaining -= principalPart;

                schedule.ScheduleItems.Add(new ScheduleItemEntity
                {
                    PaymentNumber = i,
                    PaymentDate = startDate.AddMonths(i),
                    PaymentAmount = monthlyPayment,
                    InterestAmount = interest,
                    PrincipalAmount = principalPart,
                    RemainingPrincipal = remaining
                });
            }
        }
        else
        {
            _logger.LogError("Неизвестный тип платежа: {PaymentType}", paymentType);
            throw new ArgumentException($"Тип платежа '{paymentType}' не поддерживается");
        }

        await _repository.SaveAsync(schedule);
        return schedule.ScheduleId;
    }
}
