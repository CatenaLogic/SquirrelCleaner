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
