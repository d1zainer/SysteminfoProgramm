namespace SystemProgramm.Models;

// Одно показание для карточки. Load - заполненность в процентах,
// null означает "нет данных", а не ноль.
public sealed record HardwareReading(
    string Headline,
    string? Detail = null,
    double? Load = null,
    string? LoadText = null);