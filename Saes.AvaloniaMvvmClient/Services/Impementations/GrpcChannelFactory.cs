using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Interceptors;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class GrpcChannelFactory : IGrpcChannelFactory
    {
        private readonly string _address;
        private readonly IServiceProvider _sp;

        public GrpcChannelFactory(string address, IServiceProvider sp)
        {
            _address = address;
            _sp = sp;
        }

        public CallInvoker CreateChannel()
        {
            var channel = GrpcChannel.ForAddress(_address);
//#if DEBUG
            //return channel.Intercept(_sp.GetService<StatusLoggingInterceptor>());
//#else
    return channel.Intercept(_sp.GetService<SessionKeyInterceptor>())
                    .Intercept(_sp.GetService<StatusLoggingInterceptor>());
//#endif

        }
    }
}
