 using Grpc.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
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
                var response = userSessionClient.GetUserByCurrentSession(new Google.Protobuf.WellKnownTypes.Empty());

                var userClient = new Saes.Protos.ModelServices.UserService.UserServiceClient(_grpcChannel);

                var request = new UserLookup { UserId = response.User.UserId };
                var response2 = userClient.GetRights(request);

                _rights.Clear();

                foreach (var right in response2.Data)
                {
                    _rights.Add(right);
                }

            }
            catch (Exception ex)
            {
                await MessageBoxHelper.Exception("Исключение", $"Во время попытки получить права пользователя произошла ошибка");
            }
        }
    }
}
