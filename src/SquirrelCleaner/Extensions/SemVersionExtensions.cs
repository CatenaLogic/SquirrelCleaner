// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SemVersionExtensions.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner
{
    using Catel;
    using Semver;

    public static class SemVersionExtensions
    {
        public static bool IsPrerelease(this SemVersion version)
        {
            Argument.IsNotNull(() => version);

            if (!string.IsNullOrWhiteSpace(version.Prerelease))
            {
                return true;
            }

            return false;
        }
    }
}