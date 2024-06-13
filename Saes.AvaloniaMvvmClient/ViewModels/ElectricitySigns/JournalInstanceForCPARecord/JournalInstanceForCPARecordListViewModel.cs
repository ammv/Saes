using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord
{
    public class JournalInstanceForCPARecordListViewModel : ViewModelTabListBase<JournalInstanceForCPARecordDto, JournalInstanceForCPARecordLookup>
    {
        private CallInvoker _grpcChannel;
        private readonly IDialogService _dialogService;

        public JournalInstanceForCPARecordListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Журнал поэкземплярного учета СКЗИ для органа криптографической защиты";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();

            ClearCommand = ReactiveCommand.Create(() => { Lookup = new JournalInstanceForCPARecordLookup(); });
            _dialogService = dialogService;
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<JournalInstanceForCPARecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCPARecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (!await MessageBoxHelper.Question("Вопрос",
                $"Вы уверены, что хотите удалить данную запись с № {_selectedEntity.JournalInstanceForCPARecordId} ?")) return;

            try
            {
                var client = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление записи журнал поэкземплярного учета СКЗИ для ОКЗ"));
                var response = await client.RemoveAsync(new JournalInstanceForCPARecordLookup { JournalInstanceForCPARecordID = SelectedEntity.JournalInstanceForCPARecordId });
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

                if (response.Result)
                {
                    MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                }
                else
                {
                    MessageBus.Current.SendMessage(StatusData.Error("Ошибка"));
                }
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }

            await _Search();
        }

        protected override async Task OnEditCommand()
        {
            if (SelectedEntity == null) return;

            var vm = App.ServiceProvider.GetService<JournalInstanceForCPARecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCPARecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;

            var vm = App.ServiceProvider.GetService<JournalInstanceForCPARecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCPARecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
        }

        protected override Task _Export()
        {
            throw new NotImplementedException();
        }

        public ReactiveCommand<Unit, Unit> ClearCommand { get;  }
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
