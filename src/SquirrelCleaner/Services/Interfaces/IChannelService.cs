namespace SquirrelCleaner.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    internal interface IChannelService
    {
        Task<IEnumerable<Channel>> FindChannelsAsync(string channelsRoot);
        bool IsChannel(string directory);
    }
}
