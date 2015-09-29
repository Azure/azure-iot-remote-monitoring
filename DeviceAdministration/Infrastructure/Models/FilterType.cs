namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Defines different types of filters that can be set on a column
    /// </summary>
    public enum FilterType
    {
        // filter on status values--not a string filter!
        Status,

        // STRING FILTER VALUES FOLLOW

        // Full match ("ab" matches "ab" but not "abc" or "cab")
        ExactMatchCaseSensitive,
        ExactMatchCaseInsensitive,

        // Starts with ("ab" matches "ab" as well as "abcdef" but not "zab")
        StartsWithCaseSensitive,
        StartsWithCaseInsensitive,

        // Contains ("ab" matches "cab", "abc", and "ab" but not "ac")
        ContainsCaseSensitive,
        ContainsCaseInsensitive
    }
}
