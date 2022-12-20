namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Models;

    internal static class ICleanerServiceExtensions
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public static async Task CleanAsync(this ICleanerService cleanerService, IEnumerable<Channel> channels, bool isFakeClean, Action completedCallback = null)
        {
            ArgumentNullException.ThrowIfNull(cleanerService);

            var cleanedUpChannels = new List<Channel>();

            var channelsToCleanUp = (from channel in channels
                                     where channel.IsIncluded
                                     select channel).ToList();

            Log.Info("Cleaning up '{0}' channels", channelsToCleanUp.Count);

            foreach (var channel in channelsToCleanUp)
            {
                // Note: we can also do them all async (don't await), but the disk is probably the bottleneck anyway
                await cleanerService.CleanAsync(channel, isFakeClean);

                cleanedUpChannels.Add(channel);

                if (completedCallback is not null)
                {
                    completedCallback();
                }
            }

            Log.Info("Cleaned up '{0}' channels", cleanedUpChannels.Count);
        }
    }
}
