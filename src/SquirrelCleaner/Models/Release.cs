// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Release.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Models
{
    using Catel;
    using Semver;
    using System;
    using System.Collections.Generic;
    using Catel.Data;

    public class Release : ModelBase, IEquatable<Release>
    {
        public Release(Channel channel, string fileName, SemVersion version)
        {
            Argument.IsNotNull(() => channel);
            Argument.IsNotNull(() => fileName);
            Argument.IsNotNull(() => version);

            Channel = channel;
            FileName = fileName;
            Version = version;
        }

        public Channel Channel { get; private set; }

        public string FileName { get; private set; }

        public SemVersion Version { get; private set; }

        public string FullPackageFileName
        {
            get { return $"{Channel.Product}-{Version}-full.nupkg"; }
        }

        public string FullLineInReleasesFile { get; set; }

        public string DeltaPackageFileName
        {
            get { return $"{Channel.Product}-{Version}-delta.nupkg"; }
        }

        public string DeltaLineInReleasesFile { get; set; }

        public override string ToString()
        {
            return $"{Channel} {Version}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Release);
        }

        public bool Equals(Release other)
        {
            return other is not null &&
                   EqualityComparer<SemVersion>.Default.Equals(Version, other.Version);
        }

        public override int GetHashCode()
        {
            return -1677367089 + EqualityComparer<SemVersion>.Default.GetHashCode(Version);
        }

        public static bool operator ==(Release release1, Release release2)
        {
            return EqualityComparer<Release>.Default.Equals(release1, release2);
        }

        public static bool operator !=(Release release1, Release release2)
        {
            return !(release1 == release2);
        }
    }
}