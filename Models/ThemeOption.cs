namespace SystemProgramm.Models;

public sealed class ThemeOption(string id, string name)
{
    public string Id { get; } = id;

    public string Name { get; } = name;
}
