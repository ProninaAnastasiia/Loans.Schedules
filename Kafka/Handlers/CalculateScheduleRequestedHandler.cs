using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Services;
using Newtonsoft.Json;

namespace Loans.Schedules.Kafka.Handlers;

public class CalculateScheduleRequestedHandler : IEventHandler<CalculateRepaymentScheduleEvent>
{
    private readonly ILogger<CalculateScheduleRequestedHandler> _logger;
    private readonly IScheduleCalculationService _calculator;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateScheduleRequestedHandler(ILogger<CalculateScheduleRequestedHandler> logger, IScheduleCalculationService calculator,IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _calculator = calculator;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateRepaymentScheduleEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleId = _calculator.CalculateRepaymentAsync(contractEvent.ContractId, contractEvent.LoanAmount,
                contractEvent.LoanTermMonths, contractEvent.InterestRate, contractEvent.PaymentType, cancellationToken);
            
            var @event = new ContractScheduleCalculatedEvent(contractEvent.ContractId, scheduleId.Result, contractEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateSchedule"];
    
            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateScheduleRequested. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}",contractEvent.ContractId , contractEvent.OperationId, e.Message);
        }
    }
}