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
                return timestamp.ToDateTime().ToLocalTime().ToString();
            }
            return new BindingNotification(new InvalidCastException($"Cant casting {value.GetType().Name} to DateTime"), BindingErrorType.Error);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is DateTime dateTime) && dateTime != null)
            {
                return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime());
            }
            if(value is null)
            {
                return new BindingNotification(new InvalidCastException($"Value was Null"), BindingErrorType.Error);
            }
            return new BindingNotification(new InvalidCastException($"Cant casting {value.GetType().Name} to Timestamp"), BindingErrorType.Error);
        }
    }
}
