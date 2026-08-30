using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public interface ISectionReader
{
    SectionKind Section { get; }

    IReadOnlyList<MetricInfo> Metrics { get; }
    
    SectionReading ReadSection();
}