namespace SystemProgramm.Models;

public sealed class PageInfo(string title, PageKind kind)
{
    public string Title { get; } = title;

    public PageKind Kind { get; } = kind;
}