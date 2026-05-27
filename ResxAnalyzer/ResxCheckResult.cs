namespace System.Resources
{
    /// <summary>
    /// Contains the outcome of one analyzer check.
    /// </summary>
    /// <param name="CheckName">The check type name.</param>
    /// <param name="Succeeded"><c>true</c> when the check found no problems.</param>
    /// <param name="Report">The printable report for this check.</param>
    public sealed record ResxCheckResult(string CheckName, bool Succeeded, string Report)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResxCheckResult"/> class.
        /// </summary>
        /// <param name="succeeded"><c>true</c> when the check found no problems.</param>
        /// <param name="report">The printable report for this check.</param>
        public ResxCheckResult(bool succeeded, string report)
            : this(string.Empty, succeeded, report)
        {
        }

        internal ResxCheckResult WithCheckName(string checkName)
        {
            return this with { CheckName = checkName };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Report;
        }
    }
}
