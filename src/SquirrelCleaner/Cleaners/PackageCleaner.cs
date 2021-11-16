// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageCleaner.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Cleaners
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Humanizer;
    using Models;
    using Orc.FileSystem;
    using Path = Catel.IO.Path;

    public class PackageCleaner : CleanerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public PackageCleaner(IDirectoryService directoryService, IFileService fileService)
            : base(directoryService, fileService)
        {
        }

        protected override bool CanCleanChannel(Channel channel)
        {
            // For now, always allow cleaning because of RELEASES file
            return true;

            //var releasesToPurge = GetReleasesToPurge(channel);
            //return releasesToPurge.Count > 0;
        }

        protected override long CalculateCleanableSpaceForChannel(Channel channel)
        {
            // Always 1 because of RELEASES file
            var size = 1L;

            var releasesToPurge = GetReleasesToPurge(channel);

            foreach (var releaseToPurge in releasesToPurge)
            {
                var releaseSize = 0L;

                var deltaPackageFileName = Path.Combine(channel.Directory, releaseToPurge.DeltaPackageFileName);
                if (_fileService.Exists(deltaPackageFileName))
                {
                    var fileInfo = new FileInfo(deltaPackageFileName);
                    size += fileInfo.Length;
                    releaseSize += fileInfo.Length;
                }

                var fullPackageFileName = Path.Combine(channel.Directory, releaseToPurge.FullPackageFileName);
                if (_fileService.Exists(fullPackageFileName))
                {
                    var fileInfo = new FileInfo(fullPackageFileName);
                    size += fileInfo.Length;
                    releaseSize += fileInfo.Length;
                }

                Log.Info($"Found release that can be purged: '{releaseToPurge}' ({releaseSize.Bytes().Humanize("#.#")})");
            }

            return size;
        }

        protected override async Task CleanChannelAsync(Channel channel, bool isFakeClean)
        {
            var releasesFileName = Path.Combine(channel.Directory, "RELEASES");
            var releasesFileContents = new List<string>();

            foreach (var release in channel.Releases)
            {
                var deltaFileName = Path.Combine(channel.Directory, release.DeltaPackageFileName);
                var fullFileName = Path.Combine(channel.Directory, release.FullPackageFileName);

                if (!ShouldReleaseBePurged(release))
                {    
                    if (_fileService.Exists(deltaFileName))
                    {
                        var line = release.DeltaLineInReleasesFile;
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            Log.Warning($"Line is missing for DELTA release package '{release}'");
                        }
                        else
                        {
                            releasesFileContents.Add(line);
                        }
                    }

                    if (_fileService.Exists(fullFileName))
                    {
                        var line = release.FullLineInReleasesFile;
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            Log.Warning($"Line is missing for FULL release package '{release}'");
                        }
                        else
                        {
                            releasesFileContents.Add(line);
                        }
                    }
                }
                else
                {
                    if (_fileService.Exists(deltaFileName))
                    {
                        Log.Debug($"Deleting file '{deltaFileName}'");

                        if (!isFakeClean)
                        {
                            _fileService.Delete(deltaFileName);
                        }
                    }

                    if (_fileService.Exists(fullFileName))
                    {
                        Log.Debug($"Deleting file '{fullFileName}'");

                        if (!isFakeClean)
                        {
                            _fileService.Delete(fullFileName);
                        }
                    }
                }
            }

            Log.Debug($"Updating releases file '{releasesFileName}'");

            if (!isFakeClean)
            {
                var contents = string.Join("\n", releasesFileContents);
                await _fileService.WriteAllTextAsync(releasesFileName, contents);
            }
        }

        private List<Release> GetReleasesToPurge(Channel channel)
        {
            var releases = new List<Release>();

            var lastStableRelease = channel.LastStableRelease;
            if (lastStableRelease is not null)
            {
                releases.AddRange(from release in channel.Releases
                                  where ShouldReleaseBePurged(release)
                                  select release);
            }

            return releases;
        }

        private bool ShouldReleaseBePurged(Release release)
        {
            if (!release.Version.IsPrerelease())
            {
                return false;
            }

            // Note: we keep the unstable packages of the last stable release + any upcoming release
            if (release.Version >= release.Channel.LastStableRelease)
            {
                return false;
            }

            return true;
        }
    }
}
