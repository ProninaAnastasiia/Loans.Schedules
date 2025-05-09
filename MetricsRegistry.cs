using System.Collections.Concurrent;
using Prometheus;

namespace Loans.Schedules;

public static class MetricsRegistry
{
    public static readonly Histogram LoanProcessingDuration = Metrics
        .CreateHistogram("schedule_calculation_duration_seconds", "Время расчета графика платежей", 
            new HistogramConfiguration
            {
                Buckets = new double[] { 1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60 }
            });

    private static readonly ConcurrentDictionary<Guid, IDisposable> Timers = new();

    public static void StartTimer(Guid operationId)
    {
        var timer = LoanProcessingDuration.NewTimer();
        Timers.TryAdd(operationId, timer);
    }

    public static void StopTimer(Guid operationId)
    {
        if (Timers.TryRemove(operationId, out var timer))
        {
            timer.Dispose();
        }
    }
}