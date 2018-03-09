// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICleanerService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cleaners;
    using Models;

    internal interface ICleanerService
    {
        event EventHandler<ChannelEventArgs> ChannelCleaning;
        event EventHandler<ChannelEventArgs> ChannelCleaned;

        #region Methods
        IEnumerable<ICleaner> GetAvailableCleaners();

        Task CleanAsync(Channel channel, bool isFakeClean);
        #endregion

        Task<bool> CanCleanAsync(Channel channel);
    }
}