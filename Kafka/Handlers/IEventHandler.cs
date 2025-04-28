namespace Loans.Schedules.Kafka.Handlers;

public interface IEventHandler<T>
{
    Task HandleAsync(T contractEvent, CancellationToken cancellationToken);
}