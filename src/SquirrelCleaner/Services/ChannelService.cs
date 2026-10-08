namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using MethodTimer;
    using Models;
    using Microsoft.Extensions.Logging;
    using Orc.FileSystem;
    using Semver;

    internal class ChannelService : IChannelService
    {
        private readonly ILogger<ChannelService> _logger;

        private readonly ICleanerService _cleanerService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        public ChannelService(ICleanerService cleanerService, IDirectoryService directoryService,
            IFileService fileService, ILogger<ChannelService> logger)
        {
            ArgumentNullException.ThrowIfNull(cleanerService);
            ArgumentNullException.ThrowIfNull(directoryService);
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);

            _cleanerService = cleanerService;
            _directoryService = directoryService;
            _fileService = fileService;
            _logger = logger;
        }

        [Time]
        public virtual async Task<IEnumerable<Channel>> FindChannelsAsync(string channelsRoot)
        {
            Argument.IsNotNullOrWhitespace(() => channelsRoot);

            _logger.LogInformation("Searching for channels in root {ChannelsRoot}", channelsRoot);

            if (!_directoryService.Exists(channelsRoot))
            {
                _logger.LogWarning("Directory {ChannelsRoot} does not exist, cannot find any channels", channelsRoot);
                return Enumerable.Empty<Channel>();
            }

            var cleanableChannels = new List<Channel>();

            foreach (var directory in _directoryService.GetDirectories(channelsRoot, "*", SearchOption.AllDirectories))
            {
                if (IsChannel(directory))
                {
                    var channel = new Channel(directory, _cleanerService.GetAvailableCleaners());

                    var releasesFileName = Path.Combine(directory, "RELEASES");

                    var releaseFileContent = await _fileService.ReadAllTextAsync(releasesFileName);
                    var releasesFileLines = releaseFileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var files = _directoryService.GetFiles(directory, "*.nupkg", SearchOption.TopDirectoryOnly);
                    var releases = new List<Release>();

                    foreach (var file in files)
                    {
                        try
                        {
                            if (file.EndsWithIgnoreCase(".nupkg"))
                            {
                                var version = file.ExtractVersionFromFileName();
                                var release = new Release(channel, file, version);

                                var deltaCheck = $"{version}-delta.nupkg";
                                release.DeltaLineInReleasesFile = (from line in releasesFileLines
                                                                   where line.ContainsIgnoreCase(deltaCheck)
                                                                   select line).FirstOrDefault();

                                var fullCheck = $"{version}-full.nupkg";
                                release.FullLineInReleasesFile = (from line in releasesFileLines
                                                                  where line.ContainsIgnoreCase(fullCheck)
                                                                  select line).FirstOrDefault();

                                if (!releases.Contains(release))
                                {
                                    releases.Add(release);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process file {FileName}", file);
                        }
                    }

                    channel.Releases.AddRange(releases.OrderBy(x => x.Version, SemVersion.SortOrderComparer));

                    channel.LastStableRelease = channel.Releases
                                                    .Where(x => !x.Version.IsPrerelease())
                                                    .OrderByDescending(x => x.Version, SemVersion.SortOrderComparer)
                                                    .Select(x => x.Version).FirstOrDefault();

                    _logger.LogDebug("Found channel {Channel} with {ReleaseCount} releases, last stable release {LastStableRelease}", channel, channel.Releases.Count, channel.LastStableRelease);

                    cleanableChannels.Add(channel);
                }
            }

            _logger.LogInformation("Found {ChannelCount} channels in root {ChannelsRoot}", cleanableChannels.Count, channelsRoot);

            return cleanableChannels;
        }

        public virtual bool IsChannel(string directory)
        {
            // We have several rules out of the box to determine if a directory is a channel

            _logger.LogDebug("Checking if {Directory} is a channel", directory);

            var releasesFile = Path.Combine(directory, "RELEASES");
            if (File.Exists(releasesFile))
            {
                _logger.LogDebug("Directory {Directory} is a channel because it contains a RELEASES file in the root", directory);
                return true;
            }

            _logger.LogDebug("Directory {Directory} is not considered a channel", directory);

            return false;
        }
    }
}
