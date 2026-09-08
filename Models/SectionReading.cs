namespace SystemProgramm.Models;

// Какой график есть у раздела. Maximum = null - шкала подбирается по данным.
// Заголовок сюда не кладём: он локализованный, а ридеры живут всё время работы
// приложения и переживают смену языка.
public sealed record MetricInfo(MetricKind Kind, double? Maximum);

public sealed record DetailRow(string Name, string? Value);

// Points идут в том же порядке, что и Metrics у ридера. Secondary - вторая линия
// того же графика (приём и отдача на одной шкале сети), у большинства разделов её нет.
public sealed record SectionReading(
    IReadOnlyList<double?> Points,
    IReadOnlyList<DetailRow> Details,
    IReadOnlyList<double?>? Secondary = null);