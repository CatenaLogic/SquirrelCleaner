namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Models;
    using Microsoft.Extensions.Logging;

    internal static class ICleanerServiceExtensions
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ICleanerServiceExtensions));

        public static async Task CleanAsync(this ICleanerService cleanerService, IEnumerable<Channel> channels, bool isFakeClean, Action completedCallback = null)
        {
            ArgumentNullException.ThrowIfNull(cleanerService);

            var cleanedUpChannels = new List<Channel>();

            var channelsToCleanUp = (from channel in channels
                                     where channel.IsIncluded
                                     select channel).ToList();

            Logger.LogInformation("Cleaning up {ChannelCount} channels", channelsToCleanUp.Count);

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

            Logger.LogInformation("Cleaned up {ChannelCount} channels", cleanedUpChannels.Count);
        }
    }
}
