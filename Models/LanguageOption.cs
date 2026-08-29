namespace SystemProgramm.Models;

public sealed class LanguageOption(string culture, string name)
{
    public string Culture { get; } = culture;

    public string Name { get; } = name;
}
