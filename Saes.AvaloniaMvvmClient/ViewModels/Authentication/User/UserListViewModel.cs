using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Views.Authentication.User;
using System.Linq;
using Saes.AvaloniaMvvmClient.Core.Attributes;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authentication.User
{
    [RightScope("user_see")]
    public class UserListViewModel : ViewModelTabListBase<UserDto, UserLookup>
    {
        public UserListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService) : base()
        {
            TabTitle = "Список пользователей";
            _grpcChannel = grpcChannelFactory.CreateChannel();

            UserRoleCollection = new CollectionWithSelection<UserRoleDto>();
            _dialogService = dialogService;
        }

        private readonly CallInvoker _grpcChannel;
        private readonly IDialogService _dialogService;

        [Reactive]
        public CollectionWithSelection<UserRoleDto> UserRoleCollection { get; set; }

        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task _Search()
        {
            var userServiceClient = new UserService.UserServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение пользователей"));
                var response = await userServiceClient.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<UserDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _Export()
        {
            var fileName = await FileDialogHelper.SaveExcel("Сохранение пользователей", Entities);
            if (await MessageBoxHelper.Question("Вопрос", $"Файл {fileName} успешно сохранен. Желаете его открыть?"))
            {
                FileDialogHelper.Open(fileName);
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                await _Search();
                var service = new UserRoleService.UserRoleServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение ролей пользователей"));
                var response = await service.SearchAsync(new UserRoleLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                UserRoleCollection.Items.Clear();
                foreach (var item in response.Data)
                {
                    UserRoleCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;
            var vm = App.ServiceProvider.GetService<UserFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<UserFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, null, new UserDto());

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите удалить данную запись с № п/п {SelectedEntity.UserId}?"))
            {
                try
                {
                    var service = new UserService.UserServiceClient(_grpcChannel);
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление пользователя"));
                    var response = await service.RemoveAsync(new UserLookup { UserId = SelectedEntity.UserId });
                    if (response.Result)
                    {
                        MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                        await MessageBoxHelper.Success("Уведомление", $"Запись с № п/п {SelectedEntity.UserId} успешно удалена!");
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
            var vm = App.ServiceProvider.GetService<UserFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }
    }
}
