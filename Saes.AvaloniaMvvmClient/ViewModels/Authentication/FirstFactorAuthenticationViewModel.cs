using Grpc.Core;
using Grpc.Net.Client;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization
{
    public class FirstFactorAuthenticationViewModel: ViewModelBase
    {
        private string _login;

        public string Login
        {
            get { return _login; }
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private bool _authCommandExecuting = false;
        private readonly IGrpcChannelFactory _grpcChannelFactory;

        private bool AuthCommandExecuting
        {
            get { return _authCommandExecuting; }
            set => this.RaiseAndSetIfChanged(ref _authCommandExecuting, value);
        }


        public ReactiveCommand<Unit, FirstFactorAuthenticateResponse> AuthCommand { get; }

        public FirstFactorAuthenticationViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            var isValidObservable = this.WhenAnyValue(x => x.Login, x => x.Password,
                (login, password) => !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password));
            AuthCommand = ReactiveCommand.CreateFromTask(AuthCommandOnExecute, isValidObservable);
            _grpcChannelFactory = grpcChannelFactory;
        }

        private async Task<FirstFactorAuthenticateResponse> AuthCommandOnExecute()
        {
            AuthCommandExecuting = true;

            var authService = new Authentication.AuthenticationClient(_grpcChannelFactory.CreateChannel());

            var firstFactorAuthenticateRequest = new FirstFactorAuthenticateRequest
            {
                Login = _login,
                Password = _password
            };

            FirstFactorAuthenticateResponse firstFactorAuthenticateResponse;

            try
            {
                firstFactorAuthenticateResponse = await authService.FirstFactorAuthenticateAsync(firstFactorAuthenticateRequest);
            }
            catch (RpcException ex)
            {
                return null;
            }

            AuthCommandExecuting = false;

            return firstFactorAuthenticateResponse;
        }
    }
}
