using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class TimestampToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((value is Google.Protobuf.WellKnownTypes.Timestamp timestamp) && timestamp != null)
            {

                if(targetType == typeof(DateTime))
                {
                    return timestamp.ToDateTime().ToLocalTime();
                }
                if (targetType == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset(timestamp.ToDateTime().ToLocalTime());
                }
                if (targetType == typeof(DateTimeOffset?))
                {
                    return new DateTimeOffset?(timestamp.ToDateTime().ToLocalTime());
                }
                return timestamp.ToDateTime().ToLocalTime();

            }
            if(value == null)
            {
                return null;
            }
            return new BindingNotification(new InvalidCastException($"Cant casting {value?.GetType().Name} to DateTime"), BindingErrorType.Error);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is DateTime dateTime) && dateTime != null)
            {
                return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime());
            }
            if ((value is DateTimeOffset dateTimeOffset) && dateTimeOffset != null)
            {
                return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTimeOffset.DateTime.ToUniversalTime());
            }
            if (value is null)
            {
                return null;
            }
            return new BindingNotification(new InvalidCastException($"Cant casting {value.GetType().Name} to Timestamp"), BindingErrorType.Error);
        }
    }
}
