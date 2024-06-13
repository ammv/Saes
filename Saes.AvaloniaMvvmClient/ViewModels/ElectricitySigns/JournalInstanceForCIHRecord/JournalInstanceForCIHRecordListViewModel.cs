using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Core.Attributes;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHRecord
{
    [RightScope("journal_instance_for_cih_record_see")]
    public class JournalInstanceForCIHRecordListViewModel : ViewModelTabListBase<JournalInstanceForCIHRecordDto, JournalInstanceForCIHRecordLookup>
    {
        private CallInvoker _grpcChannel;
        private readonly IDialogService _dialogService;

        public JournalInstanceForCIHRecordListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Журнал поэкземплярного учета СКЗИ, эксплуатационной и технической документации к ним, ключевых документов (для обладателя конфиденциальной информации)";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            _dialogService = dialogService;
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCIHRecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
        }

        protected override Task OnCopyCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnDeleteCommand()
        {
            throw new NotImplementedException();
        }

        protected override async Task OnEditCommand()
        {
            var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCIHRecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
        }

        protected override async Task OnSeeCommand()
        {
            var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.JournalInstanceForCIHRecordId} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);
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
            var client = new JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<JournalInstanceForCIHRecordDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
