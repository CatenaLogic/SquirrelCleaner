namespace SquirrelCleaner.Services
{
    using System;
    using Catel;
    using Models;

    internal class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            Channel = channel;
        }

        public Channel Channel { get; set; }
    }
}
