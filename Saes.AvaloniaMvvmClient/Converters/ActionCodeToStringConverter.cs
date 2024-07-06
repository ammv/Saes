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
        private string GetFullName(string code)
        {
            switch (code)
            {
                case "C": return "Create";
                case "U": return "Update";
                case "D": return "Delete";
                case "R": return "Recovery";
                default: throw new Exception($"Invalid char code - {code}");
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if(value is string s)
            {
                return GetFullName(s);
            }
            else if(value is char ch)
            {
                return GetFullName(ch.ToString());
            }  
            throw new Exception($"Invalid value type - {value.GetType()}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
