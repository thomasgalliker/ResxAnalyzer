namespace System.Resources
{
    /// <summary>
    /// Represents an expected or discovered localized .resx file.
    /// </summary>
    /// <param name="CultureName">The culture represented by the localized resource file.</param>
    /// <param name="FilePath">The localized .resx file path.</param>
    /// <param name="Exists"><c>true</c> when the file exists.</param>
    public sealed record ResxLocalizedResourceFile(
        string CultureName,
        string FilePath,
        bool Exists);
}
