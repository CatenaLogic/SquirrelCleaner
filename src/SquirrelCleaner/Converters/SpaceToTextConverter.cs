﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpaceToTextConverter.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Converters
{
    using System;
    using Catel.MVVM.Converters;
    using Humanizer;

    public class SpaceToTextConverter : ValueConverterBase<long?>
    {
        protected override object Convert(long? value, Type targetType, object parameter)
        {
            if (!value.HasValue || value.Value == 0L)
            {
                return string.Empty;
            }

            return value.Value.Bytes().Humanize("#.#");
        }
    }
}