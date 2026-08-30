namespace SystemProgramm.Models;

// Какой график есть у раздела. Maximum = null - шкала подбирается по данным.
public sealed record MetricInfo(MetricKind Kind, string Title, double? Maximum);

public sealed record DetailRow(string Name, string? Value);

// Points идут в том же порядке, что и Metrics у ридера.
public sealed record SectionReading(IReadOnlyList<double?> Points, IReadOnlyList<DetailRow> Details);