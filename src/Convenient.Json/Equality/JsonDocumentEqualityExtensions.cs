using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Convenient.Json.Equality;

public static class JsonDocumentEqualityExtensions
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
        var path = new JsonEqualityError();
        return firstElement.DeepEquals(secondElement, path);   
    }
    
    public static bool DeepEquals(this JsonElement firstElement, JsonElement secondElement, [MaybeNullWhen(true)] out JsonEqualityError error)
    {
        error = new JsonEqualityError();
        if (firstElement.DeepEquals(secondElement, error))
        {
            error = default;
            return true;
        }

        return false;
    }

    private static bool DeepEquals(this JsonElement firstElement, JsonElement secondElement, JsonEqualityError error)
    {
        if (firstElement.ValueKind != secondElement.ValueKind)
        {
            error.Fail("Different value kinds");
            return false;
        }
        
        switch (firstElement.ValueKind, secondElement.ValueKind)
        {
            case (JsonValueKind.Object, JsonValueKind.Object):
            {
                return firstElement.DeepEqualsObjects(secondElement, error);
            }
            case (JsonValueKind.Array, JsonValueKind.Array):
            {
                return firstElement.DeepEqualsArrays(secondElement, error);
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

                error.Fail("Values differ");
                return false;
        }

        return false;
    }

    private static bool DeepEqualsObjects(this JsonElement firstElement, JsonElement secondElement, JsonEqualityError error)
    {
        foreach (var property in firstElement.EnumerateObject())
        {
            if (!secondElement.TryGetProperty(property.Name, out var secondPropertyValue))
            {
                error.Fail("Missing property in second element");
                return false;
            }

            using (error.Enter(property.Name))
            {
                return property.Value.DeepEquals(secondPropertyValue, error);    
            }
        }

        return true;
    }

    private static bool DeepEqualsArrays(this JsonElement firstElement, JsonElement secondElement, JsonEqualityError error)
    {
        var firstEnumerator = firstElement.EnumerateArray();
        var secondEnumerator = secondElement.EnumerateArray();

        if (firstEnumerator.GetCount() != secondEnumerator.GetCount())
        {
            error.Fail("Differ in size");
            return false;
        }
        
        var ii = 0;
        foreach (var firstItem in firstElement.EnumerateArray())
        {
            secondEnumerator.MoveNext();
            var secondItem = secondEnumerator.Current;
            using (error.Enter($"[{ii}]"))
            {
                if (!firstItem.DeepEquals(secondItem, error))
                {
                    return false;
                }    
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