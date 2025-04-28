using Loans.Schedules.Data.Models;
using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka.Events;

namespace Loans.Schedules.Services;

public class RepaymentCalculationService : IRepaymentCalculationService
{
    private readonly IScheduleRepository _repository;
    private readonly ILogger<RepaymentCalculationService> _logger;
    private IRepaymentCalculationService _repaymentCalculationServiceImplementation;

    public RepaymentCalculationService(IScheduleRepository repository, ILogger<RepaymentCalculationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CalculateRepaymentAsync(CalculateRepaymentScheduleEvent contractEvent,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(10); // Можно потом заменить на дату из события

        var schedule = new ScheduleEntity
        {
            ScheduleId = Guid.NewGuid(),
            CalculationDate = DateTime.UtcNow,
            ContractId = contractEvent.ContractId
        };

        var loanAmount = contractEvent.LoanAmount;
        var interestRate = contractEvent.InterestRate / 100m / 12; // месячная ставка
        var months = contractEvent.LoanTermMonths;

        if (contractEvent.PaymentType.Equals("аннуитет", StringComparison.OrdinalIgnoreCase))
        {
            var annuityFactor = (decimal)(
                (double)interestRate * Math.Pow(1 + (double)interestRate, months) /
                (Math.Pow(1 + (double)interestRate, months) - 1)
            );
            var monthlyPayment = Math.Round(loanAmount * annuityFactor, 2);

            var remaining = loanAmount;

            for (int i = 1; i <= months; i++)
            {
                var interest = Math.Round(remaining * interestRate, 2);
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
        else if (contractEvent.PaymentType.Equals("дифференцированная", StringComparison.OrdinalIgnoreCase))
        {
            var principalPart = Math.Round(loanAmount / months, 2);
            var remaining = loanAmount;

            for (int i = 1; i <= months; i++)
            {
                var interest = Math.Round(remaining * interestRate, 2);
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
            _logger.LogError("Неизвестный тип платежа: {PaymentType}", contractEvent.PaymentType);
            throw new ArgumentException($"Тип платежа '{contractEvent.PaymentType}' не поддерживается");
        }

        await _repository.SaveAsync(schedule);
        return schedule.ScheduleId;
    }
}
