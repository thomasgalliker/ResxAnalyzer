namespace System.Resources
{
    /// <summary>
    /// Defines one reusable .resx analyzer check.
    /// </summary>
    public interface IResxCheck
    {
        /// <summary>
        /// Gets a short explanation of what the check validates.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Runs the check against the parsed resource context.
        /// </summary>
        /// <param name="context">The parsed resource context.</param>
        /// <returns>The check result.</returns>
        ResxCheckResult Analyze(ResxAnalysisContext context);
    }
}
