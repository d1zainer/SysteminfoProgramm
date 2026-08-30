using SystemProgramm.Models;
using SystemProgramm.Services.Readers;

namespace SystemProgramm.Services;

public sealed class HardwareSampler(HardwareMonitor monitor, IReadOnlyList<IHardwareReader> readers) : IDisposable
{
    private SynchronizationContext? _context;

    private CancellationTokenSource? _cancellation;

    private volatile ISectionReader? _section;

    public IReadOnlyList<IHardwareReader> Readers => readers;

    public IReadOnlyList<HardwareReading>? Latest { get; private set; }

    public event EventHandler<IReadOnlyList<HardwareReading>>? Updated;
    
    public event EventHandler<SectionSample>? SectionUpdated;

    public void Watch(ISectionReader? section) => _section = section;

    // Контекст берём у вызывающего: старт уходит в фон, а события должны приходить в UI-поток.
    public void Start(TimeSpan interval, SynchronizationContext? context)
    {
        if (_cancellation is not null)
            return;

        _context = context;
        _cancellation = new CancellationTokenSource();

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
            // Первый снимок сразу, не дожидаясь тика: окно уже открыто и ждёт данных.
            do
                Publish(Sample(), SampleSection());
            while (await timer.WaitForNextTickAsync(token));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private HardwareReading[] Sample()
    {
        monitor.NextTick();

        return readers.Select(reader => reader.Read()).ToArray();
    }

    private SectionSample? SampleSection()
    {
        // Раздел мог смениться прямо во время тика, поэтому кладём в посылку и его вид:
        // страница отбросит чужие данные.
        var section = _section;

        return section is null ? null : new SectionSample(section.Section, section.ReadSection());
    }

    private void Publish(IReadOnlyList<HardwareReading> readings, SectionSample? section)
    {
        if (_context is null)
        {
            Deliver(readings, section);
            return;
        }

        _context.Post(_ => Deliver(readings, section), null);
    }

    private void Deliver(IReadOnlyList<HardwareReading> readings, SectionSample? section)
    {
        Latest = readings;
        Updated?.Invoke(this, readings);

        if (section is not null)
            SectionUpdated?.Invoke(this, section);
    }
}

