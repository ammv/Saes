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

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserRole
{
    [RightScope("user_role_see")]
    public class UserRoleListViewModel : ViewModelTabListBase<UserRoleDto, UserRoleLookup>
    {
        private readonly IDialogService _dialogService;
        private CallInvoker _grpcChannel;

        public UserRoleListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Роли пользователей";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            _dialogService = dialogService;
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<UserRoleFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, null, new UserRoleDto());

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnEditCommand()
        {
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<UserRoleFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<UserRoleFormViewModel>();

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
            var client = new UserRoleService.UserRoleServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение ролей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<UserRoleDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
