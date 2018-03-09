// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelNameConverter.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2018 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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