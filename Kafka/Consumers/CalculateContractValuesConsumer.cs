using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Schedules.Kafka.Consumers;

public class CalculateContractValuesConsumer : KafkaBackgroundConsumer
{
    public CalculateContractValuesConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<CalculateContractValuesConsumer> logger)
        : base(config, serviceProvider, logger,
            topic: config["Kafka:Topics:CalculateContractValues"],
            groupId: "schedule-service-group",
            consumerName: nameof(CalculateContractValuesConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateContractValuesEvent") == true)
        {
            var @event = message.ToObject<CalculateContractValuesEvent>();
            if (@event != null) await ProcessCalculateContractValuesEventAsync(@event, cancellationToken);
        }
    }
    private async Task ProcessCalculateContractValuesEventAsync(CalculateContractValuesEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            MetricsRegistry.StartTimer(@event.OperationId);
            using var scope = ServiceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CalculateContractValuesEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CalculateContractValuesEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}