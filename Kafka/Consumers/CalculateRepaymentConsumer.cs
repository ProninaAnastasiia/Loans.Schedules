using Confluent.Kafka;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Schedules.Kafka.Consumers;

public class CalculateRepaymentConsumer: BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalculateRepaymentConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CalculateRepaymentConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CalculateRepaymentConsumer> logger)
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
        consumer.Subscribe(_configuration["Kafka:Topics:CalculateRepaymentSchedule"]);

        _logger.LogInformation("KafkaConsumerService CalculateRepaymentConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);

                var jsonObject = JObject.Parse(result.Message.Value);

                // Определяем тип события по наличию определенных свойств
                if (jsonObject.Property("EventType").Value.ToString().Contains("CalculateRepaymentScheduleEvent"))
                {
                    var @event = jsonObject.ToObject<CalculateRepaymentScheduleEvent>();
                    if (@event != null) await ProcessCalculateRepaymentScheduleEventAsync(@event, stoppingToken);
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
    
    private async Task ProcessCalculateRepaymentScheduleEventAsync(CalculateRepaymentScheduleEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CalculateRepaymentScheduleEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}