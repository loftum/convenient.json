using System.Text.Json;

namespace Convenient.Json;

public class JsonMerge
{
    public JsonMergeOptions Options { get; init; }
    public IList<JsonDocument> Documents { get; } = new List<JsonDocument>();

    public void Add(JsonDocument node)
    {
        Documents.Add(node);
    }

    public JsonDocument GetResult()
    {
        if (!Documents.Any())
        {
            throw new InvalidOperationException("No nodes");
        }

        var result = Documents.First();
        
        foreach (var document in Documents.Skip(1))
        {
            result = result.MergeWith(document, Options);
        }

        return result;
    }
}