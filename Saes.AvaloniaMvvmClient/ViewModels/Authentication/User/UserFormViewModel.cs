using Avalonia.Controls;
using Avalonia.Media;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.Views.Authentication.User;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authentication.User
{
    public class UserFormViewModel : ViewModelFormBase<UserDto, UserDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        public UserFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            UserRoleCollection = new CollectionWithSelection<UserRoleDto>();
        }
        protected override bool Validate()
        {
            throw new NotImplementedException();
        }

        [Reactive]
        public CollectionWithSelection<UserRoleDto> UserRoleCollection { get; set; }

        public bool FormModeIsAdd => _currentMode == Core.Enums.FormMode.Add;

        protected override UserDataRequest _ConfigureDataRequest(UserDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new UserDataRequest
                {
                    Login = dto.Login,
                    TwoFactorEnabled = dto.TwoFactorEnabled == true,
                    UserId = dto.UserId,
                    UserRoleId = dto.UserRoleId
                };
            }
            else
            {
                return new UserDataRequest { TwoFactorEnabled = true, };
            }

        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр пользователя";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование пользователя";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление пользователя";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                await _UserRoleCollectionLoad();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }

        }

        private async Task _UserRoleCollectionLoad()
        {

            try
            {
                var client = new UserRoleService.UserRoleServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение ролей пользователей"));
                var response = await client.SearchAsync(new UserRoleLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

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

        private async Task ShowOtpQrCode()
        {
            try
            {
                var service = new UserService.UserServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение нового OTP-токена пользователя"));
                var response = await service.UpdateTwoFactorTokenAsync(new UserLookup { UserId = DataRequest.UserId });

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                var qrCodeWindow = new QrCodeOtpWindow();
                qrCodeWindow.SetQrCode(response.UriToken);
                await qrCodeWindow.ShowDialog(WindowManager.Windows.FirstOrDefault( x => x.DataContext == this));

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка со стороны сервера", $"Во время обновления двухфакторного кода произошла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время обновления двухфакторного кода произошла неизвестная ошибка:\n{ex.Message}");
            }
        }

        protected override async Task _OnAdd()
        {
            try
            {
                var service = new UserService.UserServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление нового пользователя"));
                var response = await service.AddAsync(DataRequest);

                DataRequest.UserId = response.Data.FirstOrDefault().UserId;

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                if(_dataRequest.TwoFactorEnabled == true )
                {
                    await ShowOtpQrCode();
                }

                await MessageBoxHelper.Success("Успешно", "Пользователь успешно добавлен!");

                Callback?.Invoke(response.Data.FirstOrDefault());

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка со стороны сервера", $"Во время добавления пользователя возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления пользователя возникла неизвестная ошибка:\n{ex.Message}");
            }
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new UserService.UserServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение пользователя"));
                var response = await service.EditAsync(DataRequest);


                if (_dataRequest.TwoFactorEnabled == true && _dto.TwoFactorEnabled == false)
                {
                    await ShowOtpQrCode();
                }

                await MessageBoxHelper.Success("Успешно", "Пользователь успешно изменен!");

                MessageBus.Current.SendMessage(StatusData.Ok("Успешный успех"));

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));

                 await MessageBoxHelper.Exception("Ошибка", $"Во время изменения произошла неизвестная ошибка: {ex.Message}");
            }
        }

        protected override Task _OnSee()
        {
            throw new NotImplementedException();
        }
    }
}
