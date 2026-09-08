using System.Reflection;
using Avalonia.Platform;
using SystemProgramm.Models;

namespace SystemProgramm.ViewModels.Pages;

public sealed class AboutViewModel() : PageViewModel(new PageInfo(Localization.PageAbout, SectionKind.About))
{
    public string Version { get; } = string.Format(Localization.AboutVersion, VersionText());
    
    public string OnestLicense { get; } = Read("avares://SystemProgramm/Assets/Fonts/OFL.txt");

    public string LucideLicense { get; } = Read("avares://SystemProgramm/Assets/Licenses/Lucide.txt");

    private static string VersionText()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        return version is null ? "?" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static string Read(string uri)
    {
        using var stream = AssetLoader.Open(new Uri(uri));
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}