using Avalonia.Data.Converters;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class ActionCodeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if(value is char ch)
            {
                switch(ch)
                {
                    case 'C': return "Create";
                    case 'U': return "Update";
                    case 'D': return "Delete";
                    case 'R': return "Recovery";
                    default: throw new Exception($"Invalid char code - {ch}");
                }
            }
            else
            {
                throw new Exception($"Invalid value type - {value.GetType()}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
