using Avalonia.Data.Converters;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class UserIdToUserDtoConverter : IValueConverter
    {
        private readonly CallInvoker _grpcChannel;

        public UserIdToUserDtoConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is int id && id > 0)
            {
                try
                {
                    var client = new UserService.UserServiceClient(_grpcChannel);
                    var response = client.Search(new UserLookup { UserId = id });
                    return response.Data.FirstOrDefault()?.Login;

                }
                catch (Exception)
                {
                    return "Ошибка при запросе";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
