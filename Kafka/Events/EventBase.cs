namespace Loans.Schedules.Kafka.Events;

public abstract record EventBase
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName ?? nameof(EventBase);
}