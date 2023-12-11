namespace SquirrelCleaner.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class ChannelExtensions
    {
        private const int MaxRunningthreads = 5;

        public static async Task CalculateCleanableSpaceAsyncAndMultithreadedAsync(this List<Channel> channels, Action completedCallback = null)
        {
            var itemsPerBatch = channels.Count / MaxRunningthreads;

            foreach (var channel in channels)
            {
                await Task.Run(async () =>
                {
                    await channel.CalculateCleanableSpaceAsync();

                    if (completedCallback is not null)
                    {
                        completedCallback();
                    }
                });
            }
        }

        public static async Task<long> CalculateCleanableSpaceAsync(this Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            if (!channel.CleanableSize.HasValue)
            {
                var cleanableSize = 0L;

                foreach (var cleaner in channel.Cleaners)
                {
                    cleanableSize += await cleaner.CalculateCleanableSpaceAsync(channel);
                }

                channel.CleanableSize = cleanableSize;
            }

            return channel.CleanableSize ?? 0L;
        }
    }
}
