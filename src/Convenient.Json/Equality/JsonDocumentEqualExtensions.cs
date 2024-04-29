using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Convenient.Json.Equality;

public static class JsonDocumentEqualExtensions
{
    public static bool DeepEquals(this JsonDocument first, JsonDocument second)
    {
        var firstElement = first.RootElement;
        var secondElement = second.RootElement;
        return firstElement.DeepEquals(secondElement);
    }
    
    
    public static bool DeepEquals(this JsonDocument first, JsonDocument second, out JsonEqualityError error)
    {
        var firstElement = first.RootElement;
        var secondElement = second.RootElement;
        return firstElement.DeepEquals(secondElement, out error);
    }
    
    public static bool DeepEquals(this JsonElement firstElement, JsonElement secondElement)
    {
        var path = new PropertyPath();
        return firstElement.DeepEquals(secondElement, path);   
    }
    
    public static bool DeepEquals(this JsonElement firstElement, JsonElement secondElement, [MaybeNullWhen(true)] out JsonEqualityError error)
    {
        var path = new PropertyPath();
        if (firstElement.DeepEquals(secondElement, path))
        {
            error = default;
            return true;
        }

        error = new JsonEqualityError(path.Path, path.ErrorMessage);
        return false;
    }

    private static bool DeepEquals(this JsonElement firstElement, JsonElement secondElement, PropertyPath path)
    {
        if (firstElement.ValueKind != secondElement.ValueKind)
        {
            path.ErrorMessage = "Different value kinds";
            return false;
        }
        
        switch (firstElement.ValueKind, secondElement.ValueKind)
        {
            case (JsonValueKind.Object, JsonValueKind.Object):
            {
                return firstElement.DeepEqualsObjects(secondElement, path);
            }
            case (JsonValueKind.Array, JsonValueKind.Array):
            {
                return firstElement.DeepEqualsArrays(secondElement, path);
            }
            case (JsonValueKind.True, JsonValueKind.True):
            case (JsonValueKind.False, JsonValueKind.False):
            case (JsonValueKind.Undefined, JsonValueKind.Undefined):
            case (JsonValueKind.Null, JsonValueKind.Null):
                return true;
            case (JsonValueKind.String, JsonValueKind.String):
            case (JsonValueKind.Number, JsonValueKind.Number):
                if (firstElement.ValueEquals(secondElement.GetString()))
                {
                    return true;
                }

                path.ErrorMessage = "Values differ";
                return false;
        }

        return false;
    }

    private static bool DeepEqualsObjects(this JsonElement firstElement, JsonElement secondElement, PropertyPath path)
    {
        foreach (var property in firstElement.EnumerateObject())
        {
            if (!secondElement.TryGetProperty(property.Name, out var secondPropertyValue))
            {
                path.ErrorMessage = $"{path} missing in second element";
                return false;
            }

            using (path.Push(property.Name))
            {
                return property.Value.DeepEquals(secondPropertyValue, path);    
            }
        }

        return true;
    }

    private static bool DeepEqualsArrays(this JsonElement firstElement, JsonElement secondElement, PropertyPath path)
    {
        var firstEnumerator = firstElement.EnumerateArray();
        var secondEnumerator = secondElement.EnumerateArray();

        if (firstEnumerator.GetCount() != secondEnumerator.GetCount())
        {
            path.ErrorMessage = "Differ in size";
            return false;
        }
        
        var ii = 0;
        foreach (var firstItem in firstElement.EnumerateArray())
        {
            secondEnumerator.MoveNext();
            var secondItem = secondEnumerator.Current;
            using var _ = path.Push($"[{ii}]");
            if (!firstItem.DeepEquals(secondItem, path))
            {
                return false;
            }

            ii++;
        }

        return true;
    }

    private static int GetCount(this JsonElement.ArrayEnumerator enumerator)
    {
        return enumerator.TryGetNonEnumeratedCount(out var count) ? count : enumerator.Count();
    }
}