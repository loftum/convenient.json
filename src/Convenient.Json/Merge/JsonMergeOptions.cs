using System.Text.Json;

namespace Convenient.Json.Merge;

public class JsonMergeOptions
{
    public ArrayMergeStrategy ArrayStrategy { get; init; } = ArrayMergeStrategy.Replace;
    public NullValueMergeStrategy NullValueStrategy { get; init; } = NullValueMergeStrategy.Ignore;
    public string UnsetKeyword { get; init; } = "<unset>";
    
    
    public static JsonMergeOptions Default { get; } = new();
    public JsonCommentHandling ReadCommentHandling { get; set; }
    public int MaxDepth { get; set; }
    public bool AllowTrailingCommas { get; set; }
}