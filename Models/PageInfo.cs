namespace SystemProgramm.Models;

public sealed class PageInfo(string title, SectionKind section)
{
    public string Title { get; } = title;

    public SectionKind Section { get; } = section;
}