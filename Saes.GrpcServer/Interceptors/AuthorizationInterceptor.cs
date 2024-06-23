using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Schemas;
using Saes.Protos;
using Saes.Protos.Auth;
using Saes.Protos.ModelServices;
using Auth = Saes.Protos.Auth;

namespace Saes.GrpcServer.Interceptors
{
    public class AuthorizationInterceptor : Interceptor
    {
        private readonly ILogger _logger;
        private static SaesContext _ctx;
        private static readonly HashSet<Type> _nonInterceptableRequestTypes;

        public static  SaesContext Context
        {
            set
            {
                _ctx = value;
            }
        }

        static AuthorizationInterceptor()
        {
            _nonInterceptableRequestTypes = new HashSet<Type> {
                typeof(FirstFactorAuthenticateRequest),
                typeof(SecondFactorAuthenticateRequest),
                typeof(ValidateSessionKeyRequest),
                typeof(UserGetRightsRequest),
                typeof(HelloRequest)
            };
            _ctx = new SaesContext();
        }

        public AuthorizationInterceptor(ILogger<AuthorizationInterceptor> logger)
        {
            _logger = logger;
            _logger.LogInformation($"Context Id: {0}", _ctx.ContextId);
        }

        private void ValidateSessionAsync(Metadata.Entry? headerSessionKey)
        {
            
            if (headerSessionKey == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "The session key is not specified in the request headers"));
            }

            if (!(headerSessionKey.Value is string sessionKey) || string.IsNullOrEmpty(sessionKey))
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Invalid session key"));
            }

            var userSession = _ctx.UserSessions.FirstOrDefault(x => x.SessionKey == sessionKey);
            if (userSession == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Invalid session key"));
            }

            if (userSession.IsExpired == true)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "The session has expired"));
            }
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if(!_nonInterceptableRequestTypes.Contains(typeof(TRequest)))
            {
                var headerSessionKey = context.RequestHeaders.Get("SessionKey");
                ValidateSessionAsync(headerSessionKey);

                await _ctx.uspSetCurrentUserSessionIDAsync(headerSessionKey!.Value);
                _logger.LogInformation($"Setted current user session"); 
            }
            
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error thrown by {context.Method}.");
                throw;
            }
        }
    }
}
