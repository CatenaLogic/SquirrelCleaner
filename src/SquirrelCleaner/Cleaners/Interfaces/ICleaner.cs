﻿namespace SquirrelCleaner.Cleaners
{
    using System.Threading.Tasks;
    using Models;

    public interface ICleaner
    {
        #region Properties
        string Name { get; }
        string Description { get; }
        #endregion

        #region Methods
        bool CanClean(Channel channel);

        Task<long> CalculateCleanableSpaceAsync(Channel channel);

        Task CleanAsync(Channel channel, bool isFakeClean);
        #endregion
    }
}
