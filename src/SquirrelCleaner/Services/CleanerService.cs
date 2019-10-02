// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CleanerService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Data;
    using Catel.Logging;
    using Catel.Threading;
    using Cleaners;
    using MethodTimer;
    using Models;

    internal class CleanerService : InterfaceFinderServiceBase<ICleaner>, ICleanerService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public event EventHandler<ChannelEventArgs> ChannelCleaning;
        public event EventHandler<ChannelEventArgs> ChannelCleaned;

        public IEnumerable<ICleaner> GetAvailableCleaners()
        {
            return GetAvailableItems();
        }

        [Time]
        public async Task<bool> CanCleanAsync(Channel channel)
        {
            var canClean = false;

            Log.Debug("Checking if channel '{0}' can be cleaned", channel);
            Log.Indent();

            var cleaners = GetAvailableCleaners();

            await TaskShim.Run(() =>
            {
                foreach (var cleaner in cleaners)
                {
                    Log.Debug("Checking if channel '{0}' can be cleaned by cleaner '{1}'", channel, cleaner);


                    if (cleaner.CanClean(channel))
                    {
                        Log.Debug("Channel '{0}' can be cleaned by cleaner '{1}'", channel, cleaner);

                        canClean = true;
                        break;
                    }
                }
            });

            Log.Unindent();
            Log.Debug("Checked if channel '{0}' can be cleaned, result = {1}", channel, canClean);

            return canClean;
        }

        [Time]
        public async Task CleanAsync(Channel channel, bool isFakeClean)
        {
            ChannelCleaning?.Invoke(this, new ChannelEventArgs(channel));

            Log.Info("Cleaning channel '{0}'", channel);
            Log.Indent();

            await TaskHelper.RunAndWaitAsync(() =>
            {
                var cleaners = GetAvailableCleaners();
                foreach (var cleaner in cleaners)
                {
                    if (cleaner.CanClean(channel))
                    {
                        Log.Debug("Cleaning channel '{0}' using cleaner '{1}'", channel, cleaner);

                        cleaner.CleanAsync(channel, isFakeClean);

                        Log.Debug("Cleaned channel '{0}' using cleaner '{1}'", channel, cleaner);
                    }
                }
            });

            Log.Unindent();
            Log.Info("Cleaned channel '{0}'", channel);

            ChannelCleaned?.Invoke(this, new ChannelEventArgs(channel));
        }
    }
}
