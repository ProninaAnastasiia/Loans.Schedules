using System.Buffers;
using AutoMapper;
using Loans.Schedules.Data.Models;
using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka.Events;
using Newtonsoft.Json;

namespace Loans.Schedules.Kafka.Handlers;

public class CalculateRepaymentScheduleHandler: IEventHandler<CalculateRepaymentScheduleEvent>
{
    private readonly IScheduleRepository _repository;
    private readonly ILogger<CalculateRepaymentScheduleHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateRepaymentScheduleHandler(IScheduleRepository repository, ILogger<CalculateRepaymentScheduleHandler> logger, IMapper mapper,IConfiguration config, KafkaProducerService producer)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateRepaymentScheduleEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            Guid scheduleId = Guid.NewGuid();
            var newSchedule = new ScheduleEntity
            {
                ScheduleId = scheduleId,
            };
            await _repository.SaveAsync(newSchedule);
            
            var @event = new RepaymentScheduleCalculatedEvent(contractEvent.ContractId, scheduleId, contractEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:RepaymentScheduleCalculated"];
    
            await _producer.PublishAsync(topic, jsonMessage);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateRepaymentScheduleEvent. OperationId: {OperationId}", contractEvent.OperationId);
        }
    }
}