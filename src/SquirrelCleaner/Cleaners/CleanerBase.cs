namespace SquirrelCleaner.Cleaners
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Reflection;
    using Models;
    using Microsoft.Extensions.Logging;
    using Orc.FileSystem;

    public abstract class CleanerBase : ICleaner
    {
        protected readonly ILogger Logger;

        protected readonly IDirectoryService _directoryService;
        protected readonly IFileService _fileService;

        protected CleanerBase(IDirectoryService directoryService, IFileService fileService, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(directoryService);
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);

            _directoryService = directoryService;
            _fileService = fileService;
            Logger = logger;

            if (GetType().TryGetAttribute(out CleanerAttribute cleanerAttribute))
            {
                Name = cleanerAttribute.Name;
                Description = cleanerAttribute.Description;
            }
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool CanClean(Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            Logger.LogDebug("Checking if cleaner {CleanerType} can clean channel {Channel}", GetType(), channel);

            var canClean = CanCleanChannel(channel);

            Logger.LogDebug("Cleaner {CleanerType} can clean channel {Channel}: {CanClean}", GetType(), channel, canClean);

            return canClean;
        }

        public async Task<long> CalculateCleanableSpaceAsync(Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            if (!CanClean(channel))
            {
                return 0L;
            }

            Logger.LogDebug("Calculating cleanable space using cleaner {CleanerType} and channel {Channel}", GetType(), channel);

            var cleanableSpace = CalculateCleanableSpaceForChannel(channel);

            Logger.LogDebug("Calculated cleanable space using cleaner {CleanerType} and channel {Channel}: {CleanableSpace}", GetType(), channel, cleanableSpace);

            return cleanableSpace;
        }

        public async Task CleanAsync(Channel channel, bool isFakeClean)
        {
            ArgumentNullException.ThrowIfNull(channel);

            if (!CanClean(channel))
            {
                return;
            }

            Logger.LogInformation("Cleaning up channel {Channel} using cleaner {CleanerType}", channel, GetType());

            await CleanChannelAsync(channel, isFakeClean);

            Logger.LogInformation("Cleaned up channel {Channel} using cleaner {CleanerType}", channel, GetType());
        }

        protected string GetRelativePath(Channel channel, string path)
        {
            ArgumentNullException.ThrowIfNull(channel);

            return Path.Combine(channel.Directory, path);
        }

        protected void DeleteDirectory(string directory, bool isFakeClean)
        {
            if (!_directoryService.Exists(directory))
            {
                return;
            }

            Logger.LogDebug("Deleting directory {Directory}", directory);

            if (!isFakeClean)
            {
                try
                {
                    _directoryService.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to delete directory {Directory}", directory);
                }
            }
        }

        protected long GetDirectorySize(string directory)
        {
            var size = 0L;

            try
            {
                if (Directory.Exists(directory))
                {
                    size += (from fileName in _directoryService.GetFiles(directory, "*", SearchOption.AllDirectories)
                             select new FileInfo(fileName)).Sum(x => x.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to calculate the cleanable space for directory {Directory}", directory);
            }

            return size;
        }

        protected abstract bool CanCleanChannel(Channel channel);

        protected abstract long CalculateCleanableSpaceForChannel(Channel channel);

        protected abstract Task CleanChannelAsync(Channel channel, bool isFakeClean);
    }
}
