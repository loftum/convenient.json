namespace Convenient.Json.Equality;

internal class StackPopper : IDisposable
{
    private readonly PropertyPath _path;

    public StackPopper(PropertyPath path)
    {
        _path = path;
    }

    public void Dispose()
    {
        _path.Pop();
    }
}