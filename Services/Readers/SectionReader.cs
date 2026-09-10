using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

// Общий скелет ридера-раздела: карточка обзора, график и таблица подробностей.
// Наследник описывает железо (DescribeRows - один раз) и отдаёт показания тика (ReadSection).
public abstract class SectionReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    private IReadOnlyList<DetailRow>? _describe;

    protected HardwareMonitor Monitor { get; } = monitor;

    public abstract SectionKind Section { get; }

    public abstract string Title { get; }

    public abstract IReadOnlyList<MetricInfo> Metrics { get; }

    public abstract HardwareReading Read();

    public abstract SectionReading ReadSection();

    // Статика раздела читается один раз. Пока железо не поднялось, DescribeRows отдаёт [],
    // и мы пробуем снова на следующий тик, а не запоминаем пустой результат навсегда.
    public IReadOnlyList<DetailRow> Describe()
    {
        if (_describe is not null)
        {
            return _describe;
        }

        var rows = Rows(DescribeRows());

        if (rows.Count == 0)
        {
            return rows;
        }

        return _describe = rows;
    }

    // Описание железа до отсева пустых строк. [] - железа ещё нет.
    protected abstract IReadOnlyList<DetailRow> DescribeRows();

    // Строки без значения не показываем: пустой прочерк ничего не объясняет.
    protected static IReadOnlyList<DetailRow> Rows(IEnumerable<DetailRow> rows) =>
        [..rows.Where(row => row.Value is not null)];

    // Показания, когда железа нет: у графика null по каждой метрике, таблица пустая.
    protected SectionReading Blank() => new([..Metrics.Select(_ => (double?)null)], []);
}
