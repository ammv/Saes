using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Core.Attributes;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Impementations;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Employee
{
    [RightScope("employee_see")]
    public class EmployeeListViewModel : ViewModelTabListBase<EmployeeDto, EmployeeLookup>
    {
        private CallInvoker _grpcChannel;
        private readonly IDialogService _dialogService;

        public EmployeeListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Сотрудники";
            _grpcChannel = grpcChannelFactory.CreateChannel();

            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            EmployeePositionCollection = new CollectionWithSelection<EmployeePositionDto>();

            ClearCommand = ReactiveCommand.Create(() => { Lookup = new EmployeeLookup(); });
            _dialogService = dialogService;
        }

        public ReactiveCommand<Unit, Unit> ClearCommand { get; }
        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<EmployeePositionDto> EmployeePositionCollection { get; set; }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<EmployeeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, null, new EmployeeDto());

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите удалить данную запись с № п/п {SelectedEntity.EmployeeId}?"))
            {
                try
                {
                    var service = new EmployeeService.EmployeeServiceClient(_grpcChannel);
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление сотрудника"));
                    var response = await service.RemoveAsync(new EmployeeLookup { EmployeeID = SelectedEntity.EmployeeId });
                    if (response.Result)
                    {
                        MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                        await MessageBoxHelper.Success("Уведомление", $"Запись с № п/п {SelectedEntity.EmployeeId} успешно удалена!");
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
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<EmployeeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<EmployeeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task _Export()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task _Loaded()
        {
            await _Search();
            await _EmployeePositionCollectionLoad();
            await _OrganizationCollectionLoad();
        }

        private async Task _EmployeePositionCollectionLoad()
        {
            try
            {
                var service = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение должностей"));
                var response = await service.SearchAsync(new EmployeePositionLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                EmployeePositionCollection.Items.Clear();
                foreach (var item in response.Data)
                {
                    EmployeePositionCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        private async Task _OrganizationCollectionLoad()
        {
            try
            {
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
            var client = new EmployeeService.EmployeeServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<EmployeeDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
