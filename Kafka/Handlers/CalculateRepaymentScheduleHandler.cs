using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Services;
using Newtonsoft.Json;

namespace Loans.Schedules.Kafka.Handlers;

public class CalculateRepaymentScheduleHandler: IEventHandler<CalculateRepaymentScheduleEvent>
{
    private readonly IScheduleRepository _repository;
    private readonly ILogger<CalculateRepaymentScheduleHandler> _logger;
    private readonly IRepaymentCalculationService _calculator;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateRepaymentScheduleHandler(IScheduleRepository repository, ILogger<CalculateRepaymentScheduleHandler> logger, IRepaymentCalculationService calculator,IConfiguration config, KafkaProducerService producer)
    {
        _repository = repository;
        _logger = logger;
        _calculator = calculator;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateRepaymentScheduleEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleId = _calculator.CalculateRepaymentAsync(contractEvent, cancellationToken);
            
            var @event = new RepaymentScheduleCalculatedEvent(contractEvent.ContractId, scheduleId.Result, contractEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateScheduleResult"];
    
            await _producer.PublishAsync(topic, jsonMessage);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateRepaymentScheduleEvent. OperationId: {OperationId}", contractEvent.OperationId);
        }
    }
}