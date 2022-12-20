namespace SquirrelCleaner.Converters
{
    using System;
    using Catel.MVVM.Converters;
    using Models;

    public class ChannelNameConverter : ValueConverterBase<Channel>
    {
        protected override object Convert(Channel value, Type targetType, object parameter)
        {
            return value?.ToString();
        }
    }
}
