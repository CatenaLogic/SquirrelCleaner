// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner
{
    using System.IO;
    using Catel;
    using Semver;

    public static class StringExtensions
    {
        public static SemVersion ExtractVersionFromFileName(this string fileName)
        {
            Argument.IsNotNull(() => fileName);

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

            var version = SemVersion.Parse(versionString, false);
            return version;
        }
    }
}