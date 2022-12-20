namespace SquirrelCleaner
{
    using System;
    using System.IO;
    using Catel;
    using Semver;

    public static class StringExtensions
    {
        public static SemVersion ExtractVersionFromFileName(this string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);

            var fileInfo = new FileInfo(fileName);
            var versionString = fileInfo.Name
                                        .Replace(fileInfo.Extension, string.Empty)
                                        .Replace("-full", string.Empty)
                                        .Replace("-delta", string.Empty);

            var dashIndex = versionString.IndexOf("-");
            if (dashIndex < 0)
            {
                return null;
            }

            versionString = versionString.Substring(dashIndex + 1);

            var version = SemVersion.Parse(versionString, SemVersionStyles.Any);
            return version;
        }
    }
}
