using System.Text.Json;
using System.Text.Json.Nodes;
using Convenient.Json.Equality;
using Convenient.Json.Merge;
using Microsoft.VisualBasic.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
    public void MergeString()
    {
        var first = JsonDocument.Parse("""
                                       {
                                         "property": "originalValue"
                                       }
                                       """);

        var second = JsonDocument.Parse("""
                                          {
                                          "property": "overriddenValue"
                                          }
                                          """);
        var result = first.MergeWith(second);

        if (!result.DeepEquals(JsonDocument.Parse("""
                                                  {
                                                  "property": "overriddenValue"
                                                  }
                                                  """), out var error))
        {
            throw FailException.ForFailure(error.ToString());
        }
    }
}


public static class TestOutputHelperExtensions
{
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true
    };
    
    public static void WritePretty(this ITestOutputHelper output, object value)
    {
        output.WriteLine(JsonSerializer.Serialize(value, Pretty));
    }
}