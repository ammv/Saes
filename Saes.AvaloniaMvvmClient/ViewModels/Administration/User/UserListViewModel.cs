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

namespace Saes.AvaloniaMvvmClient.ViewModels.Administration.User
{
    public class UserListViewModel : ViewModelTabBase
    {
        private readonly CallInvoker _grpcChannel;

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }

        public UserListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Список пользователей";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            SearchCommand = ReactiveCommand.CreateFromTask(OnSearchCommand);
            ExportCommand = ReactiveCommand.CreateFromTask(OnExportCommand);

            UserRoleCollection = new CollectionWithSelection<UserRoleDto>();
            
        }

        [Reactive]
        public UserLookup Lookup { get; set; }

        [Reactive]
        public CollectionWithSelection<UserRoleDto> UserRoleCollection { get; set; }

        [Reactive]
        public ObservableCollection<UserDto> Entities { get; set; }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        [Reactive]
        public bool SearchCommandIsExecuting { get; set; }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        private async Task OnSearchCommand()
        {
            SearchCommandIsExecuting = true;

            await _Search();

            SearchCommandIsExecuting = false;
        }

        private async Task OnExportCommand()
        {
            var fileName = await FileDialogHelper.SaveExcel("Сохранение пользователей", Entities);
            if (await MessageBoxHelper.Question("Вопрос", $"Файл {fileName} успешно сохранен. Желаете его открыть?"))
            {
                FileDialogHelper.Open(fileName);
            }
        }

        private async Task _Search()
        {
            var userServiceClient = new UserService.UserServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение пользователей"));
                var response = await userServiceClient.SearchAsync(new UserLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<UserDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        public async void Loaded()
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
    }
}
