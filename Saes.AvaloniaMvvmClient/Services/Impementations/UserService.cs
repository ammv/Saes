 using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class UserService : IUserService
    {
        private HashSet<string> _rights = new HashSet<string>();
        private readonly CallInvoker _grpcChannel;

        public UserService(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }
        public IReadOnlyCollection<string> GetRights()
        {
            return _rights;
        }

        public async void LoadRights()
        {
            try
            {
                var userSessionClient = new UserSessionService.UserSessionServiceClient(_grpcChannel);
                var response = userSessionClient.GetUserByCurrentSession(new GetUserByCurrentSessionRequest());

                var userClient = new Saes.Protos.ModelServices.UserService.UserServiceClient(_grpcChannel);

                var request = new UserGetRightsRequest { UserId = response.User.UserId };
                var response2 = userClient.GetRights(request);

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                _rights.Clear();

                foreach (var right in response2.Data)
                {
                    _rights.Add(right);
                }

            }
            catch (RpcException ex)
            {
                await MessageBoxHelper.Exception("Ошибка со стороны сервера", $"Во получения прав пользователя произошла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.Exception("Ошибка", $"Во получения прав пользователя произошла неизвестная ошибка:\n{ex.Message}");
            }
        }
    }
}
