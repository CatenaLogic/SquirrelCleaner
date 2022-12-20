namespace SquirrelCleaner
{
    using System;
    using Catel;
    using Semver;

    public static class SemVersionExtensions
    {
        public static bool IsPrerelease(this SemVersion version)
        {
            ArgumentNullException.ThrowIfNull(version);

            if (!string.IsNullOrWhiteSpace(version.Prerelease))
            {
                return true;
            }

            return false;
        }
    }
}
