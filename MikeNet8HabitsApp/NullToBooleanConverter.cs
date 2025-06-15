using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Converters;

namespace MikeNet8HabitsApp
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter as string == "true";
            bool isNull = value == null;

            return inverse ? !isNull : isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}