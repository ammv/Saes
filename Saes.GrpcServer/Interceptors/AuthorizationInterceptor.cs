using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Auth = Saes.Protos.Auth;

namespace Saes.GrpcServer.Interceptors
{
    public class AuthorizationInterceptor: Interceptor
    {
        private readonly ILogger _logger;
        private readonly SaesContext _ctx;
        private static readonly HashSet<Type> _nonInterceptableRequestTypes;

        static AuthorizationInterceptor()
        {
            _nonInterceptableRequestTypes = new HashSet<Type> {
                typeof(Auth.FirstFactorAuthenticateRequest),
                typeof(Auth.SecondFactorAuthenticateRequest)
            };
        }

        public AuthorizationInterceptor(ILogger<AuthorizationInterceptor> logger, SaesContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }

        private void ValidateSessionAsync(ServerCallContext context)
        {
            var headerSessionKey = context.RequestHeaders.Get("SessionKey");
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
                ValidateSessionAsync(context);
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
