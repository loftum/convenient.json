using System.Buffers;
using System.Text.Json;

namespace Convenient.Json.Merge;

public static class JsonDocumentMergeExtensions
{
    public static JsonDocument MergeWith(this JsonDocument first, JsonDocument second, JsonMergeOptions options = null)
    {
        options ??= JsonMergeOptions.Default;
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var firstElement = first.RootElement;
        var secondElement = second.RootElement;
        switch (firstElement.ValueKind, secondElement.ValueKind)
        {
            case (JsonValueKind.Object, JsonValueKind.Object):
                MergeObjects(writer, firstElement, secondElement, options);
                break;
            case (JsonValueKind.Array, JsonValueKind.Array):
                MergeArrays(writer, firstElement, secondElement, options);
                break;
            default:
                throw new InvalidOperationException($"Cannot merge {firstElement.ValueKind} with {secondElement.ValueKind}");
        }
        
        writer.Flush();

        return JsonDocument.Parse(buffer.WrittenMemory);
    }
    
    private static void MergeObjects(Utf8JsonWriter writer, JsonElement original, JsonElement modified, JsonMergeOptions options)
    {
        writer.WriteStartObject();
        
        foreach (var property in original.EnumerateObject())
        {
            if (modified.TryGetProperty(property.Name, out var modifiedValue))
            {
                switch (modifiedValue.ValueKind)
                {
                    case JsonValueKind.Null:
                        switch (options.NullValueStrategy)
                        {
                            case NullValueMergeStrategy.Set:
                                writer.WriteNull(property.Name);
                                break;
                            case NullValueMergeStrategy.Ignore:
                                property.WriteTo(writer);
                                break;
                        }

                        break;
                    // Unset property
                    case JsonValueKind.Undefined:
                        break;
                    // Here we go
                    default:

                        if (modifiedValue.ValueKind == JsonValueKind.String && modifiedValue.ValueEquals(options.UnsetKeyword))
                        {
                            // Unset property
                            break;
                        }
                        
                        writer.WritePropertyName(property.Name);
                        var originalValue = property.Value;

                        switch (originalValue.ValueKind, modifiedValue.ValueKind)
                        {
                            case (JsonValueKind.Object, JsonValueKind.Object):
                                MergeObjects(writer, originalValue, modifiedValue, options);
                                break;
                            case (JsonValueKind.Array, JsonValueKind.Array):
                                MergeArrays(writer, originalValue, modifiedValue, options);
                                break;
                            default:
                                modifiedValue.WriteTo(writer);
                                break;
                        }
                        break;
                }
            }
            else
            {
                property.WriteTo(writer);
            }
        }

        foreach (var property in modified.EnumerateObject())
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    switch (options.NullValueStrategy)
                    {
                        case NullValueMergeStrategy.Ignore:
                            break;
                        case NullValueMergeStrategy.Set:
                            if (!original.TryGetProperty(property.Name, out _))
                            {
                                property.WriteTo(writer);
                            }
                            break;
                    }
                    break;
                default:
                    if (!original.TryGetProperty(property.Name, out _))
                    {
                        property.WriteTo(writer);
                    }
                    break;
            }
        }
        
        writer.WriteEndObject();
    }
    
    private static void MergeArrays(Utf8JsonWriter writer, JsonElement original, JsonElement modified, JsonMergeOptions options)
    {
        writer.WriteStartArray();

        switch (options.ArrayStrategy)
        {
            case ArrayMergeStrategy.Append:
                foreach (var element in original.EnumerateArray())
                {
                    element.WriteTo(writer);
                }

                foreach (var element in modified.EnumerateArray())
                {
                    element.WriteTo(writer);
                }

                break;
            case ArrayMergeStrategy.Replace:
                foreach (var element in modified.EnumerateArray())
                {
                    element.WriteTo(writer);
                }
                break;
            case ArrayMergeStrategy.Merge:

                var originalLength = original.GetArrayLength();
                var modifiedLength = modified.GetArrayLength();
                var maxLength = Math.Max(originalLength, modifiedLength);
                
                for (var ii = 0; ii < maxLength; ii++)
                {
                    if (!original.TryGetArrayElement(ii, out var originalElement))
                    {
                        modified[ii].WriteTo(writer);
                        continue;
                    }

                    if (!modified.TryGetArrayElement(ii, out var modifiedElement))
                    {
                        originalElement.WriteTo(writer);
                        continue;
                    }

                    switch (modifiedElement.ValueKind)
                    {
                        case JsonValueKind.Null:
                            switch (options.NullValueStrategy)
                            {
                                case NullValueMergeStrategy.Unset:
                                    // Property will not be written to result
                                    break;
                                case NullValueMergeStrategy.Set:
                                    modifiedElement.WriteTo(writer);
                                    break;
                                default:
                                    originalElement.WriteTo(writer);
                                    break;
                            }

                            break;
                        case JsonValueKind.Undefined:
                            continue;
                    }

                    switch (originalElement.ValueKind)
                    {
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                        case JsonValueKind.String:
                        case JsonValueKind.Number:
                        case JsonValueKind.Null:
                        case JsonValueKind.Undefined:
                            modifiedElement.WriteTo(writer);
                            break;
                        case JsonValueKind.Object:
                            MergeObjects(writer, originalElement, modifiedElement, options);
                            break;
                        case JsonValueKind.Array:
                            MergeArrays(writer, originalElement, modifiedElement, options);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown json ValueKind {originalElement.ValueKind}");
                    }
                }
                break;
            default:
                throw new InvalidOperationException($"Invalid {nameof(ArrayMergeStrategy)}: {options.NullValueStrategy}");
        }
        
        writer.WriteEndArray();
    }
}