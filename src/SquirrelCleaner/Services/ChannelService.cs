// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using MethodTimer;
    using Models;
    using Orc.FileSystem;

    internal class ChannelService : IChannelService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ICleanerService _cleanerService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        public ChannelService(ICleanerService cleanerService, IDirectoryService directoryService,
            IFileService fileService)
        {
            Argument.IsNotNull(() => cleanerService);
            Argument.IsNotNull(() => directoryService);
            Argument.IsNotNull(() => fileService);

            _cleanerService = cleanerService;
            _directoryService = directoryService;
            _fileService = fileService;
        }

        [Time]
        public virtual async Task<IEnumerable<Channel>> FindChannelsAsync(string channelsRoot)
        {
            Argument.IsNotNullOrWhitespace(() => channelsRoot);

            Log.Info("Searching for channels in root '{0}'", channelsRoot);

            if (!_directoryService.Exists(channelsRoot))
            {
                Log.Warning("Directory '{0}' does not exist, cannot find any channels", channelsRoot);
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
                            Log.Warning(ex, $"Failed to process file '{file}'");
                        }
                    }

                    channel.Releases.AddRange(from x in releases
                                              orderby x.Version
                                              select x);

                    channel.LastStableRelease = (from release in channel.Releases
                                                 where !release.Version.IsPrerelease()
                                                 orderby release.Version descending
                                                 select release.Version).FirstOrDefault();

                    Log.Debug($"Found channel '{channel}' with '{channel.Releases.Count}' releases, last stable release '{channel.LastStableRelease}'");

                    cleanableChannels.Add(channel);
                }
            }

            Log.Info("Found {0} channels in root '{1}'", cleanableChannels.Count, channelsRoot);

            return cleanableChannels;
        }

        public virtual bool IsChannel(string directory)
        {
            // We have several rules out of the box to determine if a directory is a channel

            Log.Debug("Checking if a '{0}' is a channel", directory);

            var releasesFile = Path.Combine(directory, "RELEASES");
            if (File.Exists(releasesFile))
            {
                Log.Debug("Directory '{0}' is a channel because it contains a RELEASES file in the root", directory);
                return true;
            }

            Log.Debug("Directory '{0}' is not considered a channel", directory);

            return false;
        }
    }
}
