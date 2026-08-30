using SystemProgramm.Models;
using SystemProgramm.Services.Readers;

namespace SystemProgramm.Services;

public sealed class HardwareSampler(IReadOnlyList<IHardwareReader> readers) : IDisposable
{
    private SynchronizationContext? _context;

    private CancellationTokenSource? _cancellation;

    public IReadOnlyList<IHardwareReader> Readers => readers;

    public IReadOnlyList<HardwareReading>? Latest { get; private set; }

    public event EventHandler<IReadOnlyList<HardwareReading>>? Updated;
    
    public void Start(TimeSpan interval)
    {
        if (_cancellation is not null)
            return;

        _context = SynchronizationContext.Current;
        _cancellation = new CancellationTokenSource();

        Latest = Sample();

        _ = Task.Run(() => Loop(interval, _cancellation.Token));
    }

    public void Dispose()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
    }

    private async Task Loop(TimeSpan interval, CancellationToken token)
    {
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
                Publish(Sample());
        }
        catch (OperationCanceledException)
        {
        }
    }

    private HardwareReading[] Sample() => readers.Select(reader => reader.Read()).ToArray();

    private void Publish(IReadOnlyList<HardwareReading> readings)
    {
        if (_context is null)
        {
            Deliver(readings);
            return;
        }

        _context.Post(_ => Deliver(readings), null);
    }

    private void Deliver(IReadOnlyList<HardwareReading> readings)
    {
        Latest = readings;
        Updated?.Invoke(this, readings);
    }
}