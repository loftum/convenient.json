namespace Convenient.Json.Equality;

internal class StackPopper : IDisposable
{
    private readonly JsonEqualityError _path;

    public StackPopper(JsonEqualityError path)
    {
        _path = path;
    }

    public void Dispose()
    {
        _path.Pop();
    }
}