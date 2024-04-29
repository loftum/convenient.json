namespace Convenient.Json.Equality;

internal class PropertyPath
{
    private readonly Stack<string> _stack = new();

    public string Path => string.Join('.', _stack);
    public string ErrorMessage { get; set; }
    
    public IDisposable Push(string part)
    {
        _stack.Push(part);
        return new StackPopper(this);
    }

    public override string ToString()
    {
        return string.Join('.', _stack);
    }

    public void Pop()
    {
        if (ErrorMessage == null)
        {
            _stack.Pop();
        }
    }
}