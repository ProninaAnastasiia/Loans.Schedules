using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Schedules.Kafka.Consumers;

public class CalculateScheduleConsumer : KafkaBackgroundConsumer
{
    public CalculateScheduleConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<CalculateScheduleConsumer> logger)
        : base(config, serviceProvider, logger,
            topic: config["Kafka:Topics:CalculateSchedule"],
            groupId: "schedule-service-group",
            consumerName: nameof(CalculateScheduleConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateRepaymentScheduleEvent") == true)
        {
            var @event = message.ToObject<CalculateRepaymentScheduleEvent>();
            if (@event != null) await ProcessCalculateScheduleRequestedAsync(@event, cancellationToken);
        }
    }
    private async Task ProcessCalculateScheduleRequestedAsync(CalculateRepaymentScheduleEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CalculateRepaymentScheduleEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CalculateRepaymentScheduleEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}