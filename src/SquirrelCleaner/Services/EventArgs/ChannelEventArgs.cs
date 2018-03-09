// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelEventArgs.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System;
    using Catel;
    using Models;

    internal class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(Channel channel)
        {
            Argument.IsNotNull(() => channel);

            Channel = channel;
        }

        public Channel Channel { get; set; }
    }
}