using Loans.Schedules.Data;
using Loans.Schedules.Data.Dto;
using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka;
using Loans.Schedules.Kafka.Consumers;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Loans.Schedules.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<SchedulesDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

builder.Services.AddScoped<IScheduleCalculationService, ScheduleCalculationService>();
builder.Services.AddScoped<IEventHandler<CalculateContractValuesEvent>, CalculateContractValuesHandler>();
builder.Services.AddScoped<IEventHandler<CalculateRepaymentScheduleEvent>, CalculateScheduleRequestedHandler>();

builder.Services.AddHostedService<CalculateContractValuesConsumer>();
builder.Services.AddHostedService<CalculateScheduleConsumer>();
builder.Services.AddSingleton<KafkaProducerService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Метрики HTTP
app.UseHttpMetrics();

// Экспонирование метрик на /metrics
app.MapMetrics();

app.MapPost("/api/calculate-schedule", async ([FromBody] CalculateContractValues request, CancellationToken cancellationToken, IEventHandler<CalculateContractValuesEvent> handler) =>
{
    var @event = new CalculateContractValuesEvent(request.ContractId, request.LoanAmount, request.LoanTermMonths, request.InterestRate, request.PaymentType, request.OperationId);
    await handler.HandleAsync(@event, cancellationToken);
});

app.Run();
