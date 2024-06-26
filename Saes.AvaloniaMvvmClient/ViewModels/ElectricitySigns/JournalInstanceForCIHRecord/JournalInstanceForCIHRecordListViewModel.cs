using DynamicData.Binding;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHRecord
{
    [RightScope("journal_instance_for_cih_record_see")]
    public class JournalInstanceForCIHRecordListViewModel : ViewModelTabListBase<JournalInstanceForCIHRecordDto, JournalInstanceForCIHRecordLookup>
    {
        private CallInvoker _grpcChannel;
        private readonly IDialogService _dialogService;

        public ReactiveCommand<Unit, Unit> ClearCommand { get; private set; }

        public JournalInstanceForCIHRecordListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Журнал поэкземплярного учета СКЗИ, эксплуатационной и технической документации к ним, ключевых документов для обладателя конфиденциальной информации";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            OrganizationAddCollection = new CollectionWithSelection<OrganizationDto>();
            _dialogService = dialogService;

            ClearCommand = ReactiveCommand.Create(() => { Lookup = new JournalInstanceForCIHRecordLookup(); });

            var canExecuteAddCommand = OrganizationAddCollection.WhenAnyValue(x => x.Selected).Select(selected => selected != null);

            AddCommand = ReactiveCommand.CreateFromTask(OnAddCommand, canExecuteAddCommand);
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add,null, new JournalInstanceForCIHRecordDto { OrganizationDto = OrganizationAddCollection.Selected });

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {

            if (SelectedEntity == null) return;

            if (await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите удалить данную запись с № п/п {SelectedEntity.JournalInstanceForCIHRecordId}?"))
            {
                try
                {
                    var service = new JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceClient(_grpcChannel);
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление записи журнала поэкземплярного учёта СКЗИ для ОКИ"));
                    var response = await service.RemoveAsync(new JournalInstanceForCIHRecordLookup { JournalInstanceForCIHRecordId = SelectedEntity.JournalInstanceForCIHRecordId });
                    if(response.Result)
                    {
                        MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                        await MessageBoxHelper.Success("Уведомление", $"Запись с № п/п {SelectedEntity.JournalInstanceForCIHRecordId} успешно удалена!");
                        await _Search();
                    }
                    else
                    {
                        MessageBus.Current.SendMessage(StatusData.Error("Неизвестная ошибка"));
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(StatusData.Error(ex));
                    await MessageBoxHelper.Exception("Ошибка во время удаления записи", ex.Message);
                }
            }

        }

        protected override async Task OnEditCommand()
        {
            if(SelectedEntity != null)
            {
                var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

                vm.Configure(Core.Enums.FormMode.Edit, null,SelectedEntity);

                await _dialogService.ShowDialog(vm);
            }
            
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity != null)
            {
                var vm = App.ServiceProvider.GetService<JournalInstanceForCIHRecordFormViewModel>();

                vm.Configure(Core.Enums.FormMode.See, null,SelectedEntity);

                await _dialogService.ShowDialog(vm);
            }
        }


        protected override async Task _Export()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationAddCollection { get; set; }

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
                    OrganizationAddCollection.Items.Add(item);
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
