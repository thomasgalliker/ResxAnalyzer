# ResxAnalyzer
[![Version](https://img.shields.io/nuget/v/ResxAnalyzer.svg)](https://www.nuget.org/packages/ResxAnalyzer)
[![Downloads](https://img.shields.io/nuget/dt/ResxAnalyzer.svg)](https://www.nuget.org/packages/ResxAnalyzer)
[![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

ResxAnalyzer is a small .NET library for validating `.resx` translation resources. It is designed for unit tests.

## Download and Install ResxAnalyzer
This library is available on NuGet: https://www.nuget.org/packages/ResxAnalyzer/
Use the following command to install ResxAnalyzer using the NuGet Package Manager Console:

```powershell
PM> Install-Package ResxAnalyzer
```

Or with the .NET CLI:

```bash
dotnet add package ResxAnalyzer
```

ResxAnalyzer supports .NET Standard 2.1 and higher.

## Quick Start

The analyzer returns a simple result:

```csharp
public sealed record ResxAnalysisResult(
    bool Succeeded,
    string Report,
    IReadOnlyList<ResxCheckResult> Checks);
```

Tests can print `Report` to `ITestOutputHelper` and assert `Succeeded`.
`ToString()` returns `Report`, so `Console.WriteLine(result)` is also useful.

```csharp
using System.Resources;
using System.Resources.Checks;

public sealed class StringsTests
{
    [Fact]
    public void ShouldAnalyzeStringResources()
    {
        // Arrange
        var resxAnalyzer = ResxAnalyzer
            .ForResource("FishApp.Contracts/Resources/Strings.resx")
            .WithInvariantComment("@Invariant")
            .WithChecks(checks => checks
                .Add(new CompletenessCheck())
                .Add(new PlaceholderConsistencyCheck())
                .Add(new NewlineValueCheck())
                .Add(new UnusedKeysCheck(scan => scan
                    .In("FishApp.Api")
                    .In("FishApp.Mobile/FishApp")
                    .In("FishApp.Contracts")
                    .IgnoreKeys("^CultureInfo_"))))
            .Build()

        // Act
        var result = resxAnalyzer.Analyze();

        // Assert
        this.testOutputHelper.WriteLine(result.ToString());
        result.Succeeded.Should().BeTrue(result.Report);
    }
}
```

At least one check must be configured with `WithChecks(...)`. The analyzer does not run hidden default checks.

## Available Checks

| Check | Validates | Notes |
| --- | --- | --- |
| `CompletenessCheck` | Every non-invariant neutral key exists with a non-empty value in every localized resource. | Keys marked with the configured invariant comment, default `@Invariant`, are excluded. |
| `PlaceholderConsistencyCheck` | Localized values use the same placeholders as the neutral value. | Compares exact placeholder identity, supports numeric placeholders like `{0}` and named placeholders like `{name}`, and reports malformed placeholders such as `{0` or `{!invalid}`. |
| `DuplicateKeyCheck` | Resource files do not contain duplicate `<data name="...">` entries. | Uses parsed file-order entries, so duplicate keys can be reported even though the public dictionary exposes one entry per key. |
| `EmptyNeutralValueCheck` | Neutral resource values are not empty or whitespace. | Localized empties are handled by `CompletenessCheck`. |
| `SuspiciousSameAsNeutralCheck` | Localized values are not identical to the neutral value for non-invariant keys. | Useful for copied, untranslated values. |
| `WhitespaceConsistencyCheck` | Localized values preserve leading and trailing whitespace from the neutral value. | Reports spaces, tabs, and line breaks at the edges. |
| `AcceleratorKeyCheck` | Localized values preserve accelerator key markers. | Supports common markers such as `&File` and `_File`; escaped `&&` is ignored. |
| `PunctuationConsistencyCheck` | Localized values preserve terminal punctuation from the neutral value. | Checks punctuation such as `.`, `?`, `!`, `:`, `;`, and ellipses. |
| `OrphanLocalizedKeyCheck` | Localized resources do not contain keys missing from the neutral resource. | Catches stale translated keys after neutral keys are removed or renamed. |
| `CultureFileCoverageCheck` | Every configured culture has a localized resource file. | Works with cultures configured through `WithCultures(...)` or explicit localized resources. |
| `UnusedCultureFileCheck` | Discovered localized resource files are part of the configured culture set. | Useful when stale culture files remain beside the neutral resource. |
| `KeyMaxLengthCheck` | Resource keys do not exceed a configured maximum length. | Construct with `new KeyMaxLengthCheck(maxLength)`. |
| `NewlineValueCheck` | Resource values do not start or end with newlines. | Applies to neutral and localized resources. |
| `UnusedKeysCheck` | Resource keys appear in configured source files unless explicitly ignored. | Configure source scanning on the check with `new UnusedKeysCheck(scan => ...)`; scans `.cs` and `.xaml`, excluding `bin`, `obj`, `*.Designer.cs`, and `.resx`. |

## Resource Selection

`ForResource(...)` is the single entry point for selecting resources. It accepts:

- A direct `.resx` file path.
- A directory containing one or more neutral `.resx` files.
- A glob pattern with `*` for one path segment.
- A glob pattern with `**` for any subfolder depth.

Localized resource selection has three modes:

| Configuration | Localized resources used |
| --- | --- |
| Only `ForResource(...)` | Auto-discovers localized sibling files such as `Strings.de.resx`, `Strings.fr.resx`, and `Strings.it.resx`. |
| `ForResource(...).WithCultures(...)` | Infers expected sibling files from the configured cultures, for example `de-CH` maps to `Strings.de.resx`. |
| `ForResource(...).WithLocalizedResource(...)` | Uses only the explicitly configured localized files. This is useful for non-standard file names or locations. |

When a directory or glob is used, localized resources such as `Strings.de.resx` are treated as localized siblings, not as separate neutral resource groups.

```csharp
var result = ResxAnalyzer
    .ForResource("FishApp.Contracts/Resources")
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

If the directory contains multiple resource groups, all neutral resources are analyzed:

```text
Resources/Strings.resx
Resources/Strings.de.resx
Resources/OtherStrings.resx
Resources/OtherStrings.de.resx
```

Use a direct file or glob when you want to limit which groups are analyzed:

```csharp
var result = ResxAnalyzer
    .ForResource("FishApp.Contracts/Resources/Other*.resx")
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

Recursive globs can scan resource folders across a project:

```csharp
var result = ResxAnalyzer
    .ForResource("**/Resources/*.resx")
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

## Culture Discovery

`WithCultures(...)` is optional. If no cultures and no explicit localized resources are configured, the analyzer discovers sibling resources next to the neutral file:

```text
Resources/Strings.resx
Resources/Strings.de.resx
Resources/Strings.fr.resx
Resources/Strings.it.resx
```

This is enough:

```csharp
var result = ResxAnalyzer
    .ForResource("Resources/Strings.resx")
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

That call analyzes `Strings.resx` together with discovered siblings such as `Strings.de.resx` and `Strings.fr.resx`.

## Explicit Cultures

Use `WithCultures(...)` when the supported languages should be defined by the application rather than by available files:

```csharp
using System.Globalization;

var result = ResxAnalyzer
    .ForResource("Resources/Strings.resx")
    .WithCultures(new[]
    {
        new CultureInfo("de-CH"),
        new CultureInfo("fr-CH"),
        new CultureInfo("it-CH")
    })
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

When a specific culture has a parent culture, the analyzer looks for the parent resource file. For example, `de-CH` maps to `Strings.de.resx`.

This mode is stricter than discovery: if a configured culture maps to a file that does not exist, checks such as `CultureFileCoverageCheck` and `CompletenessCheck` can report that explicitly.

## Explicit Localized Resources

Use `WithLocalizedResource(...)` when localized files do not follow the default sibling naming convention:

```csharp
using System.Globalization;

var result = ResxAnalyzer
    .ForResource("Resources/Strings.resx")
    .WithLocalizedResource(new CultureInfo("de-CH"), "Translations/German.resx")
    .WithLocalizedResource(new CultureInfo("fr-CH"), "Translations/French.resx")
    .WithChecks(checks => checks.Add(new CompletenessCheck()))
    .Build()
    .Analyze();
```

Do not combine `WithCultures(...)` and `WithLocalizedResource(...)` in the same analyzer configuration.
Once explicit localized resources are configured, the analyzer uses those files as the expected localized resources instead of auto-discovering `Strings.*.resx` siblings.

## Built-In Checks

The built-in checks are regular `IResxCheck` implementations:

```csharp
.WithChecks(checks => checks
    .Add(new CompletenessCheck())
    .Add(new PlaceholderConsistencyCheck())
    .Add(new DuplicateKeyCheck())
    .Add(new EmptyNeutralValueCheck())
    .Add(new SuspiciousSameAsNeutralCheck())
    .Add(new WhitespaceConsistencyCheck())
    .Add(new AcceleratorKeyCheck())
    .Add(new PunctuationConsistencyCheck())
    .Add(new OrphanLocalizedKeyCheck())
    .Add(new CultureFileCoverageCheck())
    .Add(new UnusedCultureFileCheck())
    .Add(new KeyMaxLengthCheck(80))
    .Add(new NewlineValueCheck())
    .Add(new UnusedKeysCheck(scan => scan
        .In("MyApp")
        .IgnoreKeys("^Dynamic_"))))
```

Each check produces a `ResxCheckResult`:

```csharp
public sealed record ResxCheckResult(
    string CheckName,
    bool Succeeded,
    string Report);
```

The final `ResxAnalysisResult.Report` aggregates all failed check reports.

## Custom Checks

Custom checks implement `IResxCheck`:

```csharp
public sealed class NoTodoTranslationsCheck : IResxCheck
{
    public string Description => "Checks that translations do not contain TODO markers.";

    public ResxCheckResult Analyze(ResxAnalysisContext context)
    {
        var matches = context.AllResources
            .SelectMany(resource => resource.Entries.Values.Select(entry => new
            {
                Culture = resource.CultureName ?? "neutral",
                entry.Key,
                entry.Value
            }))
            .Where(entry => entry.Value.Contains("TODO", StringComparison.OrdinalIgnoreCase))
            .Select(entry => $"> CultureInfo \"{entry.Culture}\", Key='{entry.Key}'")
            .ToArray();

        return new ResxCheckResult(
            nameof(NoTodoTranslationsCheck),
            matches.Length == 0,
            matches.Length == 0
                ? string.Empty
                : $"Following translations contain TODO:{Environment.NewLine}{string.Join(Environment.NewLine, matches)}");
    }
}
```

Register custom checks together with built-in checks:

```csharp
var result = ResxAnalyzer
    .ForResource("Resources/Strings.resx")
    .WithChecks(checks => checks
        .Add(new CompletenessCheck())
        .Add(new PlaceholderConsistencyCheck())
        .Add(new NoTodoTranslationsCheck()))
    .Build()
    .Analyze();
```

Custom checks receive a `ResxAnalysisContext` with:

- `NeutralResource`
- `LocalizedResources`
- `AllResources`
- `AllKeys`
- `InvariantKeys`

The public resource model intentionally uses simple values:

```csharp
public sealed record ResxResource(
    string FilePath,
    string? CultureName,
    IReadOnlyDictionary<string, ResxResourceEntry> Entries);
```

File usage scanning is owned by `UnusedKeysCheck`. Custom checks receive parsed resource data, not file-scanning internals.

## JSON-Serializable Options

For standalone tools or persisted configuration, use `ResxAnalyzerOptions`. It contains only JSON-friendly values: strings, dictionaries, and lists. Checks are still configured explicitly in code after deserializing options.

```json
{
  "ResourcePath": "FishApp.Contracts/Resources/Strings.resx",
  "LocalizedResourceFiles": {
    "de-CH": "FishApp.Contracts/Resources/Strings.de.resx",
    "fr-CH": "FishApp.Contracts/Resources/Strings.fr.resx"
  },
  "InvariantComment": "@Invariant"
}
```

For discovery-based analysis, `ResourcePath` can be a directory or glob and `LocalizedResourceFiles` can be omitted:

```json
{
  "ResourcePath": "FishApp.Contracts/**/Resources/*.resx"
}
```

You can also configure one neutral file directly:

```json
{
  "ResourcePath": "FishApp.Contracts/Resources/Strings.resx"
}
```

Run from deserialized options:

```csharp
var options = JsonSerializer.Deserialize<ResxAnalyzerOptions>(json)!;
var result = ResxAnalyzer
    .ForOptions(options)
    .WithChecks(checks => checks
        .Add(new CompletenessCheck())
        .Add(new PlaceholderConsistencyCheck())
        .Add(new UnusedKeysCheck(scan => scan
            .In("FishApp.Api")
            .In("FishApp.Mobile/FishApp")
            .In("FishApp.Contracts")
            .IgnoreKeys("^CultureInfo_"))))
    .Build()
    .Analyze();

Console.WriteLine(result.Report);
return result.Succeeded ? 0 : 1;
```

If `LocalizedResourceFiles` is empty, localized sibling resources are discovered automatically. Check-specific configuration, such as source directories for `UnusedKeysCheck`, lives on the check itself rather than in `ResxAnalyzerOptions`.

## Result Report

When a check succeeds, the report includes:

```text
Check "CompletenessCheck" succeeded
Checks that every non-invariant resource key exists with a non-empty value in every localized resource.
```

When a check fails, the report includes the check name plus the explanation, for example:

```text
Check "CompletenessCheck" failed
Checks that every non-invariant resource key exists with a non-empty value in every localized resource.
Following translation keys are missing:
CultureInfo "de" (1):
> MissingButtonText

Check "PlaceholderConsistencyCheck" failed
Checks that translated values use the same placeholder tokens as the neutral resource.
Following translation keys have inconsistent placeholders:
CultureInfo "fr" (1):
> TotalMessage (neutral: {0}, {1:N2}, localized: {0})

Check "UnusedKeysCheck" failed
Checks that every resource key appears in the configured source files unless explicitly ignored.
Usage scan:
searchDirectories:
> FishApp.Api
> FishApp.Mobile/FishApp
searchFileExtensions: .cs, .xaml
ignoredUsageKeyPatterns: ^CultureInfo_

Following keys are not used (1):
> OldUnusedText
```

## Thank You

Thanks to everyone who has contributed to this project.
If you find a bug or want to propose a feature, feel free to open an issue on GitHub.

## Links

- [NuGet package](https://www.nuget.org/packages/ResxAnalyzer)
- [GitHub repository](https://github.com/thomasgalliker/ResxAnalyzer)
