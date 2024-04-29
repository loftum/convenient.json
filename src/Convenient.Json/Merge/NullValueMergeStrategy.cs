namespace Convenient.Json.Merge;

public enum NullValueMergeStrategy
{
    /// <summary>
    /// null value properties will be ignored during merging.
    /// </summary>
    Ignore = 0,
    /// <summary>null value properties will be set to null.</summary>
    Set = 1,
    /// <summary>null value properties will remove property.</summary>
    Unset = 2,
}