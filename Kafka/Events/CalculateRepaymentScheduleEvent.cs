namespace Loans.Schedules.Kafka.Events;

public record CalculateRepaymentScheduleEvent(
    Guid ContractId, decimal LoanAmount, int LoanTermMonths,
    decimal InterestRate, string PaymentType, Guid OperationId) : EventBase;