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
    public class EmployeeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if(value is EmployeeDto employee)
            {
                return $"{employee.MiddleName} {employee.FirstName} {employee.LastName}";
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
