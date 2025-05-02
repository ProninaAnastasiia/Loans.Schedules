namespace Loans.Schedules.Kafka.Events;

public record ContractScheduleCalculatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;