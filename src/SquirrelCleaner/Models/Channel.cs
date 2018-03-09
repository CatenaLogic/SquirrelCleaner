// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Models
{
    using System.Collections.Generic;
    using System.IO;
    using Catel;
    using Catel.Data;
    using Cleaners;
    using Semver;

    public class Channel : ModelBase
    {
        public Channel(string directory, IEnumerable<ICleaner> cleaners)
        {
            Argument.IsNotNull(() => directory);

            var directoryInfo = new DirectoryInfo(directory);

            IsIncluded = true;
            Product = directoryInfo.Parent.Name;
            Name = directoryInfo.Name;
            Directory = directoryInfo.FullName;
            Releases = new List<Release>();
            Cleaners = new List<ICleaner>();

            if (cleaners != null)
            {
                Cleaners.AddRange(cleaners);
            }
        }

        public bool IsIncluded { get; set; }

        public string Product { get; set; }

        public string Name { get; set; }

        public string Directory { get; set; }

        public SemVersion LastStableRelease { get; set; }

        public List<Release> Releases { get; private set; }

        public List<ICleaner> Cleaners { get; private set; }

        public long? CleanableSize { get; set; }

        public bool MatchesFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            if (Product.ContainsIgnoreCase(filter))
            {
                return true;
            }

            if (Name.ContainsIgnoreCase(filter))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{Product} => {Name}";
        }
    }
}