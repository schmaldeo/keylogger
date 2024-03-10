using System.Collections;

namespace klog;

public class LogBuffer : IEnumerable<string>
{
    private readonly List<string> _buffer = new();
    private readonly List<ILogger> _outputs = new();
    private uint MaxSize { get; }

    public LogBuffer(uint maxSize = 10)
    {
        MaxSize = maxSize;
    }
    
    /// <summary>
    /// Adds a string to the buffer.
    /// </summary>
    /// <param name="item">The string to be added</param>
    public void Add(string item)
    {
        _buffer.Add(item);
        if (IsOverflown())
        {
            Write();
        }
    }

    /// <summary>
    /// Clears the buffer.
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();
    }
    
    /// <summary>
    /// Writes the buffer to all the outputs and clears it.
    /// </summary>
    public void Write()
    {
        foreach (var output in _outputs)
        {
            output.Write(this);
        }
        Clear();
    }

    /// <summary>
    /// Adds an ILogger output to which the buffer will be written once it overflows.
    /// </summary>
    /// <param name="logger">The logger to be used</param>
    public void AddOutput(ILogger logger)
    {
        _outputs.Add(logger);
    }

    /// <summary>
    /// Indicates whether the buffer has reached its max size.
    /// </summary>
    /// <returns>True if the max size has been reached</returns>
    private bool IsOverflown()
    {
        return _buffer.Count >= MaxSize;
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public IEnumerator<string> GetEnumerator()
    {
        return _buffer.GetEnumerator();
    }
}