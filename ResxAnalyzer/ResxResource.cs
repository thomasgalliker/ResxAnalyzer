namespace Superdev.ResxAnalyzer
{
    /// <summary>
    /// Represents one parsed .resx resource file.
    /// </summary>
    /// <param name="FilePath">The source .resx file path.</param>
    /// <param name="CultureName">The localized culture name, or <c>null</c> for the neutral resource.</param>
    /// <param name="Entries">Resource entries keyed by resource key.</param>
    /// <param name="AllEntries">All resource entries in file order, including duplicate keys.</param>
    public sealed record ResxResource(
        string FilePath,
        string? CultureName,
        IReadOnlyDictionary<string, ResxResourceEntry> Entries,
        IReadOnlyList<ResxResourceEntry> AllEntries);
}
