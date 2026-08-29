namespace SystemProgramm.Models;

public sealed class PageInfo(string title, IconKind icon)
{
    public string Title { get; } = title;

    public IconKind Icon { get; } = icon;
}