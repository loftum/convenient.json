using System.Text.Json;

namespace Convenient.Json;

internal static class JsonElementExtensions
{
    public static bool TryGetArrayElement(this JsonElement array, int index, out JsonElement item)
    {
        item = default;
        if (index >= array.GetArrayLength())
        {
            return false;
        }

        item = array[index];
        return true;
    }
}