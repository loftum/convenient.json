namespace Convenient.Json.Equality;

public class JsonEqualityError
{
    private readonly Stack<string> _stack = new();

    public string PropertyPath => string.Join('.', _stack);
    public string ErrorMessage { get; internal set; }
    
    internal void Push(string part)
    {
        if (ErrorMessage == null)
        {
            _stack.Push(part);    
        }
    }

    internal void Pop()
    {
        if (ErrorMessage == null)
        {
            _stack.Pop();
        }
    }
    
    public override string ToString()
    {
        return $"{ErrorMessage} at {PropertyPath}";
    }
}

internal static class JsonEqualityErrorExtensions
{
    public static void Fail(this JsonEqualityError error, string errorMessage)
    {
        if (error == null)
        {
            return;
        }

        error.ErrorMessage = errorMessage;
    }
    
    public static IDisposable Enter(this JsonEqualityError error, string part)
    {
        if (error == null)
        {
            return WasteBasket;
        }

        error.Push(part);
        return new StackPopper(error);
    }
    
    private static readonly TrashCan WasteBasket = new();

    private class TrashCan : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

