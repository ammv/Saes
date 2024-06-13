using Avalonia.Data.Converters;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class RightToVisibilityConverter : IValueConverter
    {
        private readonly CallInvoker _grpcChannel;
        public RightToVisibilityConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
