namespace Superdev.ResxAnalyzer.Tests
{
    internal static class TestDataPaths
    {
        public static DirectoryInfo GetTestDataDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "Superdev.ResxAnalyzer")))
            {
                directory = directory.Parent;
            }

            directory.Should().NotBeNull("the repository root should contain the Superdev.ResxAnalyzer directory");
            return new DirectoryInfo(Path.Combine(directory.FullName, "Tests", "Superdev.ResxAnalyzer.Tests", "TestData"));
        }

        public static DirectoryInfo GetChecksDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetTestDataDirectory().FullName, "Resources", "Checks"));
        }

        public static ResxAnalysisResult AnalyzeCheck(string resourceName, Action<ResxChecksBuilder> configureChecks)
        {
            var checksDirectory = GetChecksDirectory();
            var analyzerBuilder = ResxAnalyzer
                .ForResource(Path.Combine(checksDirectory.FullName, $"{resourceName}.resx"))
                .WithChecks(configureChecks);

            var localizedResourceFile = Path.Combine(checksDirectory.FullName, $"{resourceName}.de.resx");
            if (File.Exists(localizedResourceFile))
            {
                analyzerBuilder.WithLocalizedResource(new CultureInfo("de-CH"), localizedResourceFile);
            }

            return analyzerBuilder
                .Build()
                .Analyze();
        }
    }
}
