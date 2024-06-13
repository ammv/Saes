using Avalonia.Data;
using Avalonia.Data.Converters;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Saes.AvaloniaMvvmClient.Converters
{
    public class HardwareToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if(value is HardwareDto hardware)
            {
                return $"{hardware.Name} ({hardware.SerialNumber})";
            }

            return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
