namespace System.Resources
{
    /// <summary>
    /// Represents one parsed .resx data entry.
    /// </summary>
    /// <param name="Key">The resource key.</param>
    /// <param name="Value">The resource value.</param>
    /// <param name="Comment">The resource comment.</param>
    public sealed record ResxResourceEntry(
        string Key,
        string Value,
        string Comment);
}