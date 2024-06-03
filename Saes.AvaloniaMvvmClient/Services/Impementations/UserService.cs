using Grpc.Core;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
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
        private UserDto _currentUser;
        private readonly CallInvoker _grpcChannel;

        public UserDto CurrentUser
        { 
            get => _currentUser;
            set => _currentUser = value; 
        }

        public UserService(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }

        public bool HasRight(string rightCode)
        {
            return true;
        }

        public void UpdateCache()
        {
            throw new NotImplementedException();
        }
    }
}
