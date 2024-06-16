using Avalonia.Data;
using Avalonia.Data.Converters;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia.Dto;
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


    public class JournalCpaRecordToReceiversStringConverter : IValueConverter
    {
        private CallInvoker _grpcChannel;
        public JournalCpaRecordToReceiversStringConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is not JournalInstanceForCPARecordDto record)
            {
                return "Неправильный тип данных";
            }

            try
            {
                var client = new JournalInstanceCPAReceiverService.JournalInstanceCPAReceiverServiceClient(_grpcChannel);

                var orgClient = new OrganizationService.OrganizationServiceClient(_grpcChannel);

                var response = client.Search(new JournalInstanceCPAReceiverLookup { RecordID = record.JournalInstanceForCPARecordId });

                var organizations = new List<OrganizationDto>();

                foreach (var receiver in response.Data)
                {
                    foreach (var org in orgClient.Search(new OrganizationLookup { BusinessEntityID = receiver.ReceiverId }).Data)
                    {
                        organizations.Add(org);

                    }
                }

                return string.Join('\n', organizations.Select(x => x.ShortName));
            }
            catch (Exception ex)
            {
                return "Не удалось загрузить список получателей";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
