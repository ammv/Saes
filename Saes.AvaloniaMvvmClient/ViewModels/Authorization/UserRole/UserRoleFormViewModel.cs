using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserRole
{
    public class UserRoleFormViewModel : ViewModelFormBase<UserRoleDto, UserRoleDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        public UserRoleFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            RightGroupExCollection = new CollectionWithSelection<RightGroupEx>();

        }
        protected override bool Validate()
        {
            return !string.IsNullOrEmpty(_dataRequest.Name);
        }

        [Reactive]
        public CollectionWithSelection<RightGroupEx> RightGroupExCollection { get; set; }
        private List<RightEx> _rightExes;
        private List<UserRoleRightDto> _userRoleRights = new List<UserRoleRightDto>();
        protected override UserRoleDataRequest _ConfigureDataRequest(UserRoleDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || _currentMode == Core.Enums.FormMode.Edit)
            {
                return new UserRoleDataRequest
                {
                    UserRoleId = dto.UserRoleId,
                    Name = dto.Name
                };
            }
            else
            {
                return new UserRoleDataRequest();
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр роли";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование роли";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление роли";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                await _RightGroupExCollectionLoad();
                if (_currentMode == Core.Enums.FormMode.See || _currentMode == Core.Enums.FormMode.Edit)
                {
                    await _ConfigureRightGroupExCollection();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task _ConfigureRightGroupExCollection()
        {
            var userRoleRightClient = new UserRoleRightService.UserRoleRightServiceClient(_grpcChannel);

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение прав ролей"));

            var response = await userRoleRightClient.SearchAsync(new UserRoleRightLookup { UserRoleId = _dto.UserRoleId });

            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));



            foreach (var item in response.Data)
            {
                var right = _rightExes.FirstOrDefault(x => x.Right?.RightId == item.RightId);
                if (right != null)
                {
                    right.Enabled = true;
                }
            }

            _userRoleRights.AddRange(response.Data);

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

        }

        private async Task _RightGroupExCollectionLoad()
        {
            var rightClient = new RightService.RightServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение прав"));
            var response = await rightClient.SearchAsync(new RightLookup());
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

            var grouppedRights = from right in response.Data
                                 orderby right.RightGroupDto.Name
                                 group right by right.RightGroupDto;

            foreach (var grouppedRight in grouppedRights)
            {
                var rights = grouppedRight.ToList();
                RightGroupEx rightGroupEx = new RightGroupEx
                {
                    RightGroup = grouppedRight.Key,
                    AddRight = new RightEx { Right = rights.FirstOrDefault(x => x.Code.EndsWith("add")) },
                    EditRight = new RightEx { Right = rights.FirstOrDefault(x => x.Code.EndsWith("edit")) },
                    DeleteRight = new RightEx { Right = rights.FirstOrDefault(x => x.Code.EndsWith("delete")) },
                    SeeRight = new RightEx { Right = rights.FirstOrDefault(x => x.Code.EndsWith("see")) },
                    ExportRight = new RightEx { Right = rights.FirstOrDefault(x => x.Code.EndsWith("export")) }
                };

                RightGroupExCollection.Items.Add(rightGroupEx);
            }

            _rightExes = RightGroupExCollection.Items.SelectMany(x => new List<RightEx>
                {
                    x.AddRight,
                    x.EditRight,
                    x.DeleteRight,
                    x.ExportRight,
                    x.SeeRight
                }).Where(x => x.Right != null).ToList();

            // RightGroupExCollection.Items = new ObservableCollection<RightGroupEx>(RightGroupExCollection.Items.OrderBy(x => x.RightGroup.Name));

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }
        protected override async Task _OnAdd()
        {
            if(!Validate())
            {
                await MessageBoxHelper.Exception("Ошибка", "Вы не заполнили название роли");
                return;
            }
            try
            {
                var userRoleClient = new UserRoleService.UserRoleServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление роли"));

                var response = await userRoleClient.AddAsync(DataRequest);

                var userRole = response.Data.FirstOrDefault();

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                await AddUserRoleRightBulk(userRole.UserRoleId);

                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

                await MessageBoxHelper.Success("Успех", "Роль успешно была добавлена");
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления роли возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления роли неизвестная ошибка:\n{ex.Message}");
            }
        }

        private async Task AddUserRoleRightBulk(int userRoleId)
        {
            var userRoleRightClient = new UserRoleRightService.UserRoleRightServiceClient(_grpcChannel);
            var addRequest = new UserRoleRightBulkRequest();

            addRequest.Data.AddRange(
            _rightExes
                .Where(x => x.Enabled)
                .Select(x => new UserRoleRightDataRequest
                {
                    UserRoleId = userRoleId,
                    RightId = x.Right.RightId
                }));

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление прав пользователя"));

            await userRoleRightClient.AddBulkAsync(addRequest);
        }

        protected override async Task _OnEdit()
        {
            if (!Validate())
            {
                await MessageBoxHelper.Exception("Ошибка", "Вы не заполнили название роли");
                return;
            }
            try
            {
                var userRoleClient = new UserRoleService.UserRoleServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение роли"));

                await userRoleClient.EditAsync(DataRequest);

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                await AddUserRoleRightBulk(_dataRequest.UserRoleId.Value);
                await RemoveUserRoleRightBulk();

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения роли возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения роли неизвестная ошибка:\n{ex.Message}");
            }
        }

        private async Task RemoveUserRoleRightBulk()
        {
            var userRoleRightClient = new UserRoleRightService.UserRoleRightServiceClient(_grpcChannel);
            var removeRequest = new UserRoleRightBulkRequest();

            removeRequest.Data.AddRange(
            _rightExes
                .Where(x => !x.Enabled)
                .Select(x => new UserRoleRightDataRequest
                {
                    UserRoleRightId = _userRoleRights.Single(y => y.RightId == x.Right.RightId).UserRoleRightId
                }));

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление прав пользователя"));

            await userRoleRightClient.RemoveBulkAsync(removeRequest);
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
