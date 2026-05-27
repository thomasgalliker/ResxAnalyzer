using System.Collections.Generic;

namespace Superdev.ResxAnalyzer
{
    /// <summary>
    /// Contains the outcome and human-readable diagnostics discovered by <see cref="ResxAnalyzer"/>.
    /// </summary>
    /// <param name="Succeeded"><c>true</c> when the analyzed resources contain no configured problems.</param>
    /// <param name="Report">A printable analysis report for test output or command-line output.</param>
    public sealed record ResxAnalysisResult(bool Succeeded, string Report, IReadOnlyList<ResxCheckResult> Checks)
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return this.Report;
        }
    }
}
