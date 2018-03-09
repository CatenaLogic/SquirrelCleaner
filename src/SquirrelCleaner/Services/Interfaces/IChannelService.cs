// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChannelService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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