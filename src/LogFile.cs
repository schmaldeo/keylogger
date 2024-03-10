using System.Reflection;

namespace klog;

public class LogFile
{
    /// <summary>
    /// Writes an informational string to the log file if it's empty.
    /// </summary>
    public static void WriteInfo(string path, StreamWriter writer)
    {
        if (new FileInfo(path).Length != 0) return;

        writer.Write($"https://github.com/schmaldeo/keylogger v{Assembly.GetEntryAssembly()!.GetName().Version} {DateTime.Now}\n\n" +
                     $"If you see something like: [CODE: xxxx], you can check what key the code represents on " +
                     $"https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes\n\n" +
                     $"Keyboard layout: {Keyboard.GetKeyboardLayout()}. Find more information about it on: " +
                     $"https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-LCID/%5bMS-LCID%5d.pdf\n" +
                     $"The modifier keys only modify the output to this file for the following characters: " +
                     $"; , . / ` [ ] \\ ', for the other ones like shift + numbers or alt graph + letters, you " +
                     $"need to find out yourself what keyboard layout it is and then just find out yourself what " +
                     $"the output was, as you can clearly see when the modifier keys are pressed and released, " +
                     $"as well as what was typed in while those were active.\n\n");
        writer.Flush();
    }
}