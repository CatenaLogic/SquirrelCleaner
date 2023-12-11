namespace SquirrelCleaner.Cleaners
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Catel.Reflection;
    using Models;
    using Orc.FileSystem;

    public abstract class CleanerBase : ICleaner
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        protected readonly IDirectoryService _directoryService;
        protected readonly IFileService _fileService;

        protected CleanerBase(IDirectoryService directoryService, IFileService fileService)
        {
            ArgumentNullException.ThrowIfNull(directoryService);
            ArgumentNullException.ThrowIfNull(fileService);

            _directoryService = directoryService;
            _fileService = fileService;

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

            Log.Debug("Checking if cleaner '{0}' can clean channel '{1}'", GetType(), channel);

            var canClean = CanCleanChannel(channel);

            Log.Debug("Cleaner '{0}' can clean channel '{1}': {2}", GetType(), channel, canClean);

            return canClean;
        }

        public async Task<long> CalculateCleanableSpaceAsync(Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            if (!CanClean(channel))
            {
                return 0L;
            }

            Log.Debug("Calculating cleanable space using cleaner '{0}' and channel '{1}'", GetType(), channel);

            var cleanableSpace = CalculateCleanableSpaceForChannel(channel);

            Log.Debug("Calculated cleanable space using cleaner '{0}' and channel '{1}': {2}", GetType(), channel, cleanableSpace);

            return cleanableSpace;
        }

        public async Task CleanAsync(Channel channel, bool isFakeClean)
        {
            ArgumentNullException.ThrowIfNull(channel);

            if (!CanClean(channel))
            {
                return;
            }

            Log.Info("Cleaning up channel '{0}' using cleaner '{1}'", channel, GetType());

            await CleanChannelAsync(channel, isFakeClean);

            Log.Info("Cleaned up channel '{0}' using cleaner '{1}'", channel, GetType());
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

            Log.Debug("Deleting directory '{0}'", directory);

            if (!isFakeClean)
            {
                try
                {
                    _directoryService.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete directory '{0}'", directory);
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
                Log.Warning(ex, "Failed to calculate the cleanable space for directory '{0}'", directory);
            }

            return size;
        }

        protected abstract bool CanCleanChannel(Channel channel);

        protected abstract long CalculateCleanableSpaceForChannel(Channel channel);

        protected abstract Task CleanChannelAsync(Channel channel, bool isFakeClean);
    }
}
