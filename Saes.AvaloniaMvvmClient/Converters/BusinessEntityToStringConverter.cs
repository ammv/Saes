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

    
    public class BusinessEntityToStringConverter : IValueConverter
    {
        private CallInvoker _grpcChannel;
        public BusinessEntityToStringConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }

        private string GetOrganizationString(int id)
        {
            try
            {
                var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);

                var response = client.Search(new OrganizationLookup { BusinessEntityID = id });

                var organization = response.Data.First();

                return organization.ShortName;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetEmployeeString(int id)
        {
            try
            {
                var client = new EmployeeService.EmployeeServiceClient(_grpcChannel);

                var response = client.Search(new EmployeeLookup { BusinessEntityID = id });

                var employee = response.Data.First();

                return $"({employee.OrganizationDto.ShortName}) {employee.MiddleName} {employee.FirstName} {employee.LastName}";
            }
            catch (Exception)
            {

                throw;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not BusinessEntityDto businessEntityDto)
            {
                return "Неправильный тип данных";
            }

            switch(businessEntityDto.BusinessEntityTypeDto.BusinessEntityTypeId)
            {
                case 1:
                    return GetOrganizationString(businessEntityDto.BusinessEntityId);
                case 2:
                    return GetEmployeeString(businessEntityDto.BusinessEntityId);
            }

            return null;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
