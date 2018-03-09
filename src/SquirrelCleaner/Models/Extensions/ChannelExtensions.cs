// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelExtensions.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Threading;

    public static class ChannelExtensions
    {
        private const int MaxRunningthreads = 5;

        public static async Task CalculateCleanableSpaceAsyncAndMultithreaded(this List<Channel> channels, Action completedCallback = null)
        {
            var itemsPerBatch = channels.Count / MaxRunningthreads;

            foreach (var channel in channels)
            {
                await TaskShim.Run(async () =>
                {
                    await channel.CalculateCleanableSpaceAsync();

                    if (completedCallback != null)
                    {
                        completedCallback();
                    }
                });
            }
        }

        public static async Task<long> CalculateCleanableSpaceAsync(this Channel channel)
        {
            Argument.IsNotNull(() => channel);

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