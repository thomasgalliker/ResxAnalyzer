using System.Text;

namespace System.Resources.Checks
{
    /// <summary>
    /// Checks that localized values preserve accelerator key markers.
    /// </summary>
    public sealed class AcceleratorKeyCheck : IResxCheck
    {
        /// <inheritdoc />
        public string Description => "Checks that localized values preserve accelerator key markers such as '&File' or '_File'.";

        /// <inheritdoc />
        public ResxCheckResult Analyze(ResxAnalysisContext context)
        {
            var mismatches = context.LocalizedResources
                .SelectMany(localizedResource => context.NeutralResource.Entries.Values
                    .Where(entry => !context.InvariantKeys.Contains(entry.Key))
                    .Where(entry => localizedResource.Entries.TryGetValue(entry.Key, out _))
                    .Select(entry =>
                    {
                        var localizedEntry = localizedResource.Entries[entry.Key];
                        return new
                        {
                            CultureName = localizedResource.CultureName ?? string.Empty,
                            entry.Key,
                            NeutralAccelerators = ExtractAcceleratorMarkers(entry.Value),
                            LocalizedAccelerators = ExtractAcceleratorMarkers(localizedEntry.Value)
                        };
                    })
                    .Where(item => !item.NeutralAccelerators.SequenceEqual(item.LocalizedAccelerators, StringComparer.Ordinal)))
                .OrderBy(item => item.CultureName, StringComparer.Ordinal)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();

            if (mismatches.Length == 0)
            {
                return new ResxCheckResult(true, string.Empty);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following localized values have inconsistent accelerator markers ({mismatches.Length}):");
            foreach (var mismatch in mismatches)
            {
                stringBuilder.AppendLine($"> CultureInfo \"{mismatch.CultureName}\", Key='{mismatch.Key}', Neutral='{FormatMarkers(mismatch.NeutralAccelerators)}', Localized='{FormatMarkers(mismatch.LocalizedAccelerators)}'");
            }

            return new ResxCheckResult(false, stringBuilder.ToString().TrimEnd());
        }

        private static string[] ExtractAcceleratorMarkers(string value)
        {
            var markers = new List<string>();
            for (var index = 0; index < value.Length - 1; index++)
            {
                if (value[index] == '&')
                {
                    if (value[index + 1] == '&')
                    {
                        index++;
                        continue;
                    }

                    markers.Add("&");
                    continue;
                }

                if (value[index] == '_' &&
                    index + 1 < value.Length &&
                    !char.IsWhiteSpace(value[index + 1]))
                {
                    markers.Add("_");
                }
            }

            return markers.ToArray();
        }

        private static string FormatMarkers(IEnumerable<string> markers)
        {
            var markerArray = markers.ToArray();
            return markerArray.Length == 0 ? "<none>" : string.Join(", ", markerArray);
        }
    }
}
