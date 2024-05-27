using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace Saes.AvaloniaMvvmClient.Interceptors
{
    public class SessionKeyInterceptor: Interceptor
    {
        private readonly ISessionKeyService _sessionKeyService;
        private static readonly ImmutableHashSet<Type> _notInterceptableRequestTypes;

        static SessionKeyInterceptor()
        {
            _notInterceptableRequestTypes = ImmutableHashSet.Create(typeof(FirstFactorAuthenticateRequest), typeof(SecondFactorAuthenticateRequest));
        }

        public SessionKeyInterceptor(ISessionKeyService sessionKeyService)
        {
            _sessionKeyService = sessionKeyService;
            
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            if(!_notInterceptableRequestTypes.Contains(typeof(TRequest)))
            {
                context.Options.Headers.Add("SessionKey", _sessionKeyService.GetSessionKey());
            }
            

            return continuation(request, context);
        }
    }
}
