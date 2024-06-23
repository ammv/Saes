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


    public class JournalCihRecordListColumnsToStringConverter : IValueConverter
    {
        private CallInvoker _grpcChannel;
        public JournalCihRecordListColumnsToStringConverter()
        {
            _grpcChannel = App.ServiceProvider.GetService<IGrpcChannelFactory>().CreateChannel();
        }

        private string InstallersToString(JournalInstanceForCIHRecordDto record)
        {
            try
            {
                var client = new JournalInstanceForCIHInstallerService.JournalInstanceForCIHInstallerServiceClient(_grpcChannel);

                var employeeClient = new EmployeeService.EmployeeServiceClient(_grpcChannel);

                var response = client.Search(new JournalInstanceForCIHInstallerLookup { RecordID = record.JournalInstanceForCIHRecordId });

                var employees = new List<EmployeeDto>();

                foreach (var receiver in response.Data)
                {
                    foreach (var emp in employeeClient.Search(new EmployeeLookup { BusinessEntityID = receiver.InstallerId }).Data)
                    {
                        employees.Add(emp);

                    }
                }

                return string.Join('\n', employees.Select(x => $"{x.MiddleName} {x.FirstName} {x.LastName}"));
            }
            catch (Exception ex)
            {
                return "Не удалось загрузить список установщиков";
            }
        }

        private string HardwaresToString(JournalInstanceForCIHRecordDto record)
        {
            try
            {
                var client = new JournalInstanceForCIHConnectedHardwareService.JournalInstanceForCIHConnectedHardwareServiceClient(_grpcChannel);

                var response = client.Search(new JournalInstanceForCIHConnectedHardwareLookup { RecordID = record.JournalInstanceForCIHRecordId });

                return string.Join('\n', response.Data.Select(x => $"{x.HardwareDto.Name} ({x.HardwareDto.SerialNumber})"));
            }
            catch (Exception ex)
            {
                return "Не удалось загрузить список подключенных устройств";
            }
        }

        private string DestructorsToString(JournalInstanceForCIHRecordDto record)
        {
            try
            {
                var client = new JournalInstanceForCIHDestructorService.JournalInstanceForCIHDestructorServiceClient(_grpcChannel);

                var employeeClient = new EmployeeService.EmployeeServiceClient(_grpcChannel);

                var response = client.Search(new JournalInstanceForCIHDestructorLookup { RecordID = record.JournalInstanceForCIHRecordId });

                var employees = new List<EmployeeDto>();

                foreach (var receiver in response.Data)
                {
                    foreach (var emp in employeeClient.Search(new EmployeeLookup { BusinessEntityID = receiver.DestructorId }).Data)
                    {
                        employees.Add(emp);

                    }
                }

                return string.Join('\n', employees.Select(x => $"{x.MiddleName} {x.FirstName} {x.LastName}"));
            }
            catch (Exception ex)
            {
                return "Не удалось загрузить список изъятелей";
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is not JournalInstanceForCIHRecordDto record)
            {
                return "Неправильный тип данных";
            }

            string paramterStr = parameter.ToString().ToLower();

            switch(paramterStr)
            {
                case "installers":
                    return InstallersToString(record);
                case "hardwares":
                    return HardwaresToString(record);
                    
                case "destructors":
                    return DestructorsToString(record);
                default:
                    throw new ArgumentException($"Invalid argument {nameof(value)} value - {value}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
