namespace SystemProgramm.Services;

// Тик опроса и ширина окна графиков связаны: окно = Window точек по Interval каждая.
// Держим их рядом, чтобы не разъезжались по файлам.
public static class Sampling
{
    public const int Window = 60;

    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
}