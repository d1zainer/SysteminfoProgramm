using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace SystemProgramm.Services;

public static class Elevation
{
    public static bool IsAdministrator { get; } = Check();
    
    public static bool Restart()
    {
        if (Environment.ProcessPath is not { } path)
            return false;

        try
        {
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Path.GetDirectoryName(path) ?? string.Empty
            });

            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static bool Check()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception)
        {
            return false;
        }
    }
}