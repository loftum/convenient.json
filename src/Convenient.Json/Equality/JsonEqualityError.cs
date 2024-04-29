namespace Convenient.Json.Equality;

public class JsonEqualityError
{
    public JsonEqualityError(string path, string errorMessage)
    {
        Path = path;
        ErrorMessage = errorMessage;
    }

    public string Path { get; }
    public string ErrorMessage { get; }

    public override string ToString()
    {
        return $"{ErrorMessage} at {Path}";
    }
}