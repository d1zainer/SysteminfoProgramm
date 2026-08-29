namespace SystemProgramm.Models;

public sealed record AppSettings
{
    public string? Language { get; init; }

    public string Theme { get; init; } = "System";
}
