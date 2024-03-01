using Windows.Win32;

namespace klog;

public class Keyboard
{
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
}