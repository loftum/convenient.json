using System.Text.Json;
using System.Text.Json.Serialization;

namespace Convenient.Json;

public class JsonMergeOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; init; }
    public ArrayMergeStrategy ArrayStrategy { get; init; } = ArrayMergeStrategy.Replace;
    public NullValueMergeStrategy NullValueStrategy { get; init; } = NullValueMergeStrategy.Ignore;
    public string UnsetKeyword { get; init; } = "<unset>";
    
    
    public static JsonMergeOptions Default { get; } = new();

    public JsonMergeOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() }
        };
        JsonSerializerOptions = jsonOptions;
    }
}