using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if(string.IsNullOrEmpty(stringValue))
                {
                    if(targetType == typeof(int?))
                    {
                        return null;
                    }
                    return 0;
                }
                else if(int.TryParse(stringValue, out var intValue))
                {
                    return intValue;
                }
            }

            return BindingOperations.DoNothing;
        }
    }
}
