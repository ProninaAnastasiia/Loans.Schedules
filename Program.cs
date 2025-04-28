using Loans.Schedules.Data;
using Loans.Schedules.Data.Mappers;
using Loans.Schedules.Data.Repositories;
using Loans.Schedules.Kafka;
using Loans.Schedules.Kafka.Consumers;
using Loans.Schedules.Kafka.Events;
using Loans.Schedules.Kafka.Handlers;
using Loans.Schedules.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<SchedulesDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

builder.Services.AddScoped<IRepaymentCalculationService, RepaymentCalculationService>();
builder.Services.AddScoped<IEventHandler<CalculateRepaymentScheduleEvent>, CalculateRepaymentScheduleHandler>();

builder.Services.AddHostedService<CalculateRepaymentConsumer>();
builder.Services.AddSingleton<KafkaProducerService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
