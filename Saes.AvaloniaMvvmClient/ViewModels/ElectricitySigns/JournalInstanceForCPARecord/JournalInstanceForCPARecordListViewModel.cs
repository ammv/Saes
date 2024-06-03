using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord
{
    public class JournalInstanceForCPARecordListViewModel : ViewModelTabListBase<JournalInstanceForCPARecordDto, JournalInstanceForCPARecordLookup>
    {
        private CallInvoker _grpcChannel;

        public JournalInstanceForCPARecordListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Журнал поэкземплярного учета СКЗИ для органа криптографической защиты";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override Task OnAddCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnCopyCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnDeleteCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnEditCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnSeeCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task _Export()
        {
            throw new NotImplementedException();
        }

        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }

        protected override async Task _Loaded()
        {
            try
            {
                await _Search();
                var service = new OrganizationService.OrganizationServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение организаций"));
                var response = await service.SearchAsync(new OrganizationLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                OrganizationCollection.Items.Clear();
                foreach (var item in response.Data)
                {
                    OrganizationCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _Search()
        {
            var client = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<JournalInstanceForCPARecordDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
