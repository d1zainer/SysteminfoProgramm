using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

/// <summary>
/// Один ридер - одна карточка. Источник данных его личное дело:
/// </summary>
public interface IHardwareReader
{
    IconKind Icon { get; }

    string Title { get; }

    HardwareReading Read();
}