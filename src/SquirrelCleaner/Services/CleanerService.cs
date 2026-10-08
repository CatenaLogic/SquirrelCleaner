namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cleaners;
    using MethodTimer;
    using Models;
    using Microsoft.Extensions.Logging;

    internal class CleanerService : InterfaceFinderServiceBase<ICleaner>, ICleanerService
    {
        private readonly ILogger<CleanerService> _logger;

        public CleanerService(IEnumerable<ICleaner> cleaners, ILogger<CleanerService> logger)
            : base(cleaners)
        {
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;
        }

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

            using var scope = _logger.BeginScope("Checking if channel {Channel} can be cleaned", channel);
            _logger.LogDebug("Checking if channel {Channel} can be cleaned", channel);

            var cleaners = GetAvailableCleaners();

            await Task.Run(() =>
            {
                foreach (var cleaner in cleaners)
                {
                    _logger.LogDebug("Checking if channel {Channel} can be cleaned by cleaner {Cleaner}", channel, cleaner);


                    if (cleaner.CanClean(channel))
                    {
                        _logger.LogDebug("Channel {Channel} can be cleaned by cleaner {Cleaner}", channel, cleaner);

                        canClean = true;
                        break;
                    }
                }
            });

            _logger.LogDebug("Checked if channel {Channel} can be cleaned, result = {CanClean}", channel, canClean);

            return canClean;
        }

        [Time]
        public async Task CleanAsync(Channel channel, bool isFakeClean)
        {
            ChannelCleaning?.Invoke(this, new ChannelEventArgs(channel));

            using var scope = _logger.BeginScope("Cleaning channel {Channel}", channel);
            _logger.LogInformation("Cleaning channel {Channel}", channel);

            await Task.Run(() =>
            {
                var cleaners = GetAvailableCleaners();
                foreach (var cleaner in cleaners)
                {
                    if (cleaner.CanClean(channel))
                    {
                        _logger.LogDebug("Cleaning channel {Channel} using cleaner {Cleaner}", channel, cleaner);

                        cleaner.CleanAsync(channel, isFakeClean);

                        _logger.LogDebug("Cleaned channel {Channel} using cleaner {Cleaner}", channel, cleaner);
                    }
                }
            });

            _logger.LogInformation("Cleaned channel {Channel}", channel);

            ChannelCleaned?.Invoke(this, new ChannelEventArgs(channel));
        }
    }
}
