using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Core.Attributes;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.EmployeePosition
{
    [RightScope("employee_position_see")]
    public class EmployeePositionListViewModel : ViewModelTabListBase<EmployeePositionDto, EmployeePositionLookup>
    {
        private readonly IDialogService _dialogService;
        private CallInvoker _grpcChannel;


        public EmployeePositionListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Должности сотрудников";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            _dialogService = dialogService;
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<EmployeePositionFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, null, new EmployeePositionDto());

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите удалить данную запись с № п/п {SelectedEntity.EmployeePositionId}?"))
            {
                try
                {
                    var service = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление должности сотрудника"));
                    var response = await service.RemoveAsync(new EmployeePositionLookup { EmployeePositionID = SelectedEntity.EmployeePositionId });
                    if (response.Result)
                    {
                        MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                        await MessageBoxHelper.Success("Уведомление", $"Запись с № п/п {SelectedEntity.EmployeePositionId} успешно удалена!");
                        await _Search();
                    }
                    else
                    {
                        MessageBus.Current.SendMessage(StatusData.Error("Неизвестная ошибка"));
                        await MessageBoxHelper.Exception("Ошибка","Не удалось удалить запись");
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
            var vm = App.ServiceProvider.GetService<EmployeePositionFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<EmployeePositionFormViewModel>();

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
        }

        protected override async Task _Search()
        {
            var client = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение должностей сотрудников"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<EmployeePositionDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
