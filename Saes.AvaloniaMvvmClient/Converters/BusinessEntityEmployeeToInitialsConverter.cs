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
    public class BusinessEntityEmployeeToInitialsConverter: IValueConverter
    {
        private CallInvoker _grpcChannel;
        public BusinessEntityEmployeeToInitialsConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }

        private string GetEmployeeString(int id)
        {
            try
            {
                var client = new EmployeeService.EmployeeServiceClient(_grpcChannel);

                var response = client.Search(new EmployeeLookup { BusinessEntityID = id });

                var employee = response.Data.First();

                return $"{employee.MiddleName} {employee.FirstName} {employee.LastName}";
            }
            catch (Exception)
            {

                throw;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return BindingOperations.DoNothing;
            if (value is not BusinessEntityDto businessEntityDto)
            {
                throw new Exception($"Invalid value type - {value.GetType()}");
            }

            switch (businessEntityDto.BusinessEntityTypeDto.BusinessEntityTypeId)
            {
                case 2:
                    return GetEmployeeString(businessEntityDto.BusinessEntityId);
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
