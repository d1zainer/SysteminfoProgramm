using Avalonia.Styling;

namespace SystemProgramm.Models;

public sealed class ThemeOption(string id, string name, ThemeVariant variant)
{
    public string Id { get; } = id;

    public string Name { get; } = name;

    public ThemeVariant Variant { get; } = variant;
}

public sealed class LanguageOption(string culture, string name)
{
    public string Culture { get; } = culture;

    public string Name { get; } = name;
}

public sealed class AppSettings
{
    public string? Language { get; set; }

    public string Theme { get; set; } = "System";
}