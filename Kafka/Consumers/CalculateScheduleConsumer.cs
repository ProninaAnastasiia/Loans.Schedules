using Confluent.Kafka;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Schedules.Kafka.Consumers;

public class CalculateScheduleConsumer: BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalculateScheduleConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CalculateScheduleConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CalculateScheduleConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken); // дать приложению прогрузиться
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "schedule-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(_configuration["Kafka:Topics:CalculateContractValues"]);

        _logger.LogInformation("KafkaConsumerService CalculateRepaymentConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                var jsonObject = JObject.Parse(result.Message.Value);

                // Определяем тип события по наличию определенных свойств
                if (jsonObject.Property("EventType").Value.ToString().Contains("CalculateContractValuesEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<CalculateContractValuesEvent>();
                    if (@event != null) await ProcessCalculateContractValuesEventAsync(@event, stoppingToken);
                }
            }
        }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "Kafka временно недоступна или ошибка получения сообщения.");
            await Task.Delay(1000, stoppingToken); // Ждем и пытаемся снова
        }
        finally
        {
            consumer.Close();
        }
    }
    
    private async Task ProcessCalculateContractValuesEventAsync(CalculateContractValuesEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            MetricsRegistry.StartTimer(@event.OperationId);
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CalculateContractValuesEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}