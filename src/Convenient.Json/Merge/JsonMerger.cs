using System.Text.Json;

namespace Convenient.Json.Merge;

public static class JsonMerger
{
    public static async Task<JsonDocument> MergeFilesAsync(IEnumerable<string> filenames, JsonMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        options ??= JsonMergeOptions.Default;

        JsonDocument result = null;
        
        foreach (var filename in filenames)
        {
            await using var stream = File.OpenRead(filename);

            var documentOptions = new JsonDocumentOptions
            {
                CommentHandling = options.ReadCommentHandling,
                MaxDepth = options.MaxDepth,
                AllowTrailingCommas = options.AllowTrailingCommas
            };
            var doc = await JsonDocument.ParseAsync(stream, documentOptions, cancellationToken);
            result = result == null
                ? doc
                : result.MergeWith(doc, options);
            
        }

        return result;
    }
}