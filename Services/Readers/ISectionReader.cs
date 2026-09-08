using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public interface ISectionReader
{
    SectionKind Section { get; }

    IReadOnlyList<MetricInfo> Metrics { get; }
    
    IReadOnlyList<DetailRow> Describe();

    SectionReading ReadSection();
}