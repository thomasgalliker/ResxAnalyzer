namespace System.Resources.Checks
{
    internal static class EdgeSequence
    {
        public static string GetLeadingHorizontalWhitespace(string value)
        {
            return GetLeading(value, IsHorizontalWhitespace);
        }

        public static string GetTrailingHorizontalWhitespace(string value)
        {
            return GetTrailing(value, IsHorizontalWhitespace);
        }

        public static string GetLeadingNewlines(string value)
        {
            return GetLeading(value, IsNewline);
        }

        public static string GetTrailingNewlines(string value)
        {
            return GetTrailing(value, IsNewline);
        }

        public static string Escape(string value)
        {
            return value
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\t", "\\t", StringComparison.Ordinal);
        }

        private static string GetLeading(string value, Func<char, bool> predicate)
        {
            var length = 0;
            while (length < value.Length && predicate(value[length]))
            {
                length++;
            }

            return length == 0 ? string.Empty : value.Substring(0, length);
        }

        private static string GetTrailing(string value, Func<char, bool> predicate)
        {
            var start = value.Length;
            while (start > 0 && predicate(value[start - 1]))
            {
                start--;
            }

            return start == value.Length ? string.Empty : value.Substring(start);
        }

        private static bool IsHorizontalWhitespace(char character)
        {
            return char.IsWhiteSpace(character) && !IsNewline(character);
        }

        private static bool IsNewline(char character)
        {
            return character is '\r' or '\n';
        }
    }
}
