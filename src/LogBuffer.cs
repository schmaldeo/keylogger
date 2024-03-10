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
    
    public void Add(string item)
    {
        _buffer.Add(item);
        if (IsOverflown())
        {
            Write();
        }
    }

    public void Clear()
    {
        _buffer.Clear();
    }
    
    public void Write()
    {
        foreach (var output in _outputs)
        {
            output.Write(this);
        }
        Clear();
    }

    public void AddOutput(ILogger logger)
    {
        _outputs.Add(logger);
    }

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