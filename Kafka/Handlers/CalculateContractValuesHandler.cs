using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Services;
using Newtonsoft.Json;

namespace Loans.Schedules.Kafka.Handlers;

public class CalculateContractValuesHandler: IEventHandler<CalculateContractValuesEvent>
{
    private readonly ILogger<CalculateContractValuesHandler> _logger;
    private readonly IScheduleCalculationService _calculator;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateContractValuesHandler(ILogger<CalculateContractValuesHandler> logger, IScheduleCalculationService calculator,IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _calculator = calculator;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateContractValuesEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleId = _calculator.CalculateRepaymentAsync(contractEvent, cancellationToken);
            
            var @event = new ContractScheduleCalculatedEvent(contractEvent.ContractId, scheduleId.Result, contractEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateContractValues"];
    
            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateContractValuesEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}",contractEvent.ContractId , contractEvent.OperationId, e.Message);
        }
    }
}