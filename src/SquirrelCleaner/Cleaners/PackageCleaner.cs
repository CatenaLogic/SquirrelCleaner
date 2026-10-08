namespace SquirrelCleaner.Cleaners
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Models;
    using Microsoft.Extensions.Logging;
    using Orc.FileSystem;

    public class PackageCleaner : CleanerBase
    {
        public PackageCleaner(IDirectoryService directoryService, IFileService fileService, ILogger<PackageCleaner> logger)
            : base(directoryService, fileService, logger)
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

                Logger.LogInformation("Found release that can be purged: {Release} ({ReleaseSize})", releaseToPurge, releaseSize.Bytes().Humanize("#.#"));
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
                            Logger.LogWarning("Line is missing for DELTA release package {Release}", release);
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
                            Logger.LogWarning("Line is missing for FULL release package {Release}", release);
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
                        Logger.LogDebug("Deleting file {FileName}", deltaFileName);

                        if (!isFakeClean)
                        {
                            _fileService.Delete(deltaFileName);
                        }
                    }

                    if (_fileService.Exists(fullFileName))
                    {
                        Logger.LogDebug("Deleting file {FileName}", fullFileName);

                        if (!isFakeClean)
                        {
                            _fileService.Delete(fullFileName);
                        }
                    }
                }
            }

            Logger.LogDebug("Updating releases file {FileName}", releasesFileName);

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
            if (release.Version.ComparePrecedenceTo(release.Channel.LastStableRelease) <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
