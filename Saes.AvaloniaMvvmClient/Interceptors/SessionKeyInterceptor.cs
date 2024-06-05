using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
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
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private static readonly ImmutableHashSet<Type> _notInterceptableRequestTypes;

        static SessionKeyInterceptor()
        {
            _notInterceptableRequestTypes = ImmutableHashSet.Create(typeof(FirstFactorAuthenticateRequest), typeof(SecondFactorAuthenticateRequest));
        }

        public SessionKeyInterceptor(ISessionKeyService sessionKeyService, IDialogService dialogService, INavigationService navigationService)
        {
            _sessionKeyService = sessionKeyService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        private ClientInterceptorContext<TRequest, TResponse> CreateContextWithSessionKey<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest: class
            where TResponse : class
        {
            var headers = new Metadata
                {
                    { "SessionKey", _sessionKeyService.GetSessionKey() }
                };

            var newOptions = context.Options.WithHeaders(headers);

            context = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                newOptions);

            return context;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            if(!_notInterceptableRequestTypes.Contains(typeof(TRequest)))
            {
                context = CreateContextWithSessionKey(context);
            }


            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(
                HandleResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner)
        {
            try
            {
                return await inner;
            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == StatusCode.PermissionDenied)
                {
                    await MessageBoxHelper.Information("Уведомление", "Срок действия вашей сесси истёк, необходимо повторно пройти аутенфикацию");

                    var contentOld = _navigationService.Content;

                    var authVm = App.ServiceProvider.GetService<AuthenticationMainViewModel>();

                    authVm.DialogMode = true;
                    authVm.CompleteCallback = () =>
                    {
                        _navigationService.NavigateTo(contentOld);
                    };

                    _navigationService.NavigateTo(authVm);

                    return await inner;

                }
                else
                {
                    throw new InvalidOperationException("Custom error", ex);
                }
                

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Custom error", ex);
            }
        }
    }
}
