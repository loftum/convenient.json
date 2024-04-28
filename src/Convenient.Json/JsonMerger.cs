using System.Text.Json;

namespace Convenient.Json;

public static class JsonMerger
{
    public static async Task<JsonDocument> MergeFilesAsync(IEnumerable<string> filenames, JsonMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        options ??= JsonMergeOptions.Default;
        var merge = new JsonMerge
        {
            Options = options
        };
        
        foreach (var filename in filenames)
        {
            await using var stream = File.OpenRead(filename);

            var documentOptions = new JsonDocumentOptions
            {
                CommentHandling = options.JsonSerializerOptions.ReadCommentHandling,
                MaxDepth = options.JsonSerializerOptions.MaxDepth,
                AllowTrailingCommas = options.JsonSerializerOptions.AllowTrailingCommas
            };
            var doc = await JsonDocument.ParseAsync(stream, documentOptions, cancellationToken);
            merge.Add(doc);
        }

        return merge.GetResult();
    }

    public static JsonElement MergeObjects(object first, object second, JsonMergeOptions options = null)
    {
        options ??= JsonMergeOptions.Default;
        using var firstDocument = JsonSerializer.SerializeToDocument(first, options.JsonSerializerOptions);
        using var secondDocument = JsonSerializer.SerializeToDocument(second, options.JsonSerializerOptions);
        using var result = firstDocument.MergeWith(secondDocument, options);
        return result.Deserialize<JsonElement>(options.JsonSerializerOptions);
    }
    
    public static T MergeObjectsTo<T>(object first, object second, JsonMergeOptions options = null)
    {
        options ??= JsonMergeOptions.Default;
        using var firstDocument = JsonSerializer.SerializeToDocument(first, options.JsonSerializerOptions);
        using var secondDocument = JsonSerializer.SerializeToDocument(second, options.JsonSerializerOptions);
        using var result = firstDocument.MergeWith(secondDocument, options);
        return result.Deserialize<T>(options.JsonSerializerOptions);
    }

    public static T Merge<T>(T first, T second, JsonMergeOptions options = null)
    {
        options ??= JsonMergeOptions.Default;
        using var firstDocument = JsonSerializer.SerializeToDocument(first, options.JsonSerializerOptions);
        using var secondDocument = JsonSerializer.SerializeToDocument(second, options.JsonSerializerOptions);
        using var result = firstDocument.MergeWith(secondDocument, options);
        return result.Deserialize<T>(options.JsonSerializerOptions);
    }
}