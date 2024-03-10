namespace klog;

public class StringBuffer
{
    private static readonly List<string> Buffer = new();
    private const int Size = 10;

    /// <summary>
    ///     Writes <see cref="System.Buffer"/> to <see cref="Program.PathToFile">the log file</see> using the passed StreamWriter
    /// <param name="writer"><see cref="StreamWriter"/> to be used</param>
    /// <param name="buffer">Optional buffer to be written</param>
    /// </summary>
    public static void Write(StreamWriter writer, List<string>? buffer = null)
    {
        var targetBuffer = buffer ?? Buffer;
        foreach (var str in targetBuffer) writer.Write(str);
        writer.Flush();
        targetBuffer.Clear();
    }

    /// <summary>
    ///     Adds a string to the <see cref="System.Buffer" /> and writes the <see cref="System.Buffer" /> to file if
    ///     <see cref="Size" /> has been reached.
    /// </summary>
    /// <param name="stringToWrite">String to add to buffer</param>
    /// <param name="writer"><see cref="StreamWriter"/> to be used</param>
    // needs to be an async void because can't make KeyboardCallback async, so cannot await inside of it
    public static void WriteToFile(string stringToWrite, StreamWriter writer)
    {
        if (Buffer.Count >= Size)
            try
            {
                Write(writer);
            }
            catch (InvalidOperationException e)
            {
                // InvalidOperationException can be thrown when the StreamWriter is currently in use, so 
                // the code waits for 1 second before writing the buffer and copies the buffer before that so there's
                // no new buffer that would be written twice to the file while the old one would be lost
                var tempBuffer = Buffer;
                Task.Delay(1000).ContinueWith(_ => Write(writer, tempBuffer));
                Console.WriteLine(e.Message);
            }

        Buffer.Add(stringToWrite);
    }

}