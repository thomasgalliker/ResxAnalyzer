namespace System.Resources.Tests
{
    internal static class TestDataPaths
    {
        public static DirectoryInfo GetTestDataDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var testDataDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "Tests", "ResxAnalyzer.Tests", "TestData"));
                if (testDataDirectory.Exists)
                {
                    return testDataDirectory;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find the test data directory.");
        }

        public static DirectoryInfo GetChecksDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetTestDataDirectory().FullName, "Resources", "Checks"));
        }
    }
}
