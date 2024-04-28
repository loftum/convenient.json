using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Convenient.Json.Tests;

public class JsonMergeTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public JsonMergeTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task MergeFiles()
    {
        using var result = await JsonMerger.MergeFilesAsync(new[] {"first.json", "second.json"});
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        var root = result.RootElement;
        Assert.Equal("overriddenValue", root.GetProperty("stringValue").GetString());
    }

    [Fact]
    public void MergeSimpleValue()
    {
        var first = new MergeObject()
        {
            StringValue = "value",
            IntValue = 42,
            BoolValue = true,
            DateTimeOffsetValue = new DateTimeOffset(1981, 04, 17, 0, 0, 0, TimeSpan.Zero),
            StringArray = ["a", "b", "c"],
            MergeObjects = new Dictionary<string, MergeObject>
            {
                ["unchanged"] = new MergeObject(),
                ["changed"] = new MergeObject(),
                ["toBeRemoved"] = new MergeObject()
            }
        };

        var second = new MergeObject()
        {
            StringValue = "value",
            IntValue = 42,
            BoolValue = true,
            DateTimeOffsetValue = new DateTimeOffset(1981, 04, 17, 0, 0, 0, TimeSpan.Zero),
            StringArray = [],
            MergeObjects = new Dictionary<string, MergeObject>
            {
                ["changed"] = new MergeObject
                {
                    IntValue = 47
                },
                ["toBeRemoved"] = null
            }
        };

        var result = JsonMerger.MergeObjectsTo<MergeObject>(first, second, new JsonMergeOptions
        {
            NullValueStrategy = NullValueMergeStrategy.Unset
        });
    }

    private static readonly JsonSerializerOptions Pretty = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    private void Print(object o)
    {
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(o, Pretty));
    }
}

public class MergeObject
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public string[] StringArray { get; set; }
    public Dictionary<string, MergeObject> MergeObjects { get; set; }
}
