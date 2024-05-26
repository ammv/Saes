using Avalonia;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using ReactiveUI;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace Saes.AvaloniaMvvmClientTest.ViewModels
{
    public class UserAuthenticationViewModel: ViewModelBase
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
        private readonly GrpcChannel _grpcChannel;

        public string Status
        {
            get { return _status; }
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private bool _authCommandExecuting = false;

        private bool AuthCommandExecuting
        {
            get { return  _authCommandExecuting; }
            set => this.RaiseAndSetIfChanged(ref _authCommandExecuting, value);
        }


        public ReactiveCommand<Unit, FirstFactorAuthenticateResponse> AuthCommand { get; }

        private async Task<FirstFactorAuthenticateResponse> AuthCommandOnExecute()
        {
            AuthCommandExecuting = true;

            Status = "Creating auth service";

            var authService = new Authentication.AuthenticationClient(_grpcChannel);

            var firstFactorAuthenticateRequest = new FirstFactorAuthenticateRequest
            {
                Login = _login,
                Password = _password
            };

            // First response

            FirstFactorAuthenticateResponse firstFactorAuthenticateResponse;

            try
            {
                Status = "firstFactorAuthenticate requesting";
                firstFactorAuthenticateResponse = await authService.FirstFactorAuthenticateAsync(firstFactorAuthenticateRequest);
                Status = "firstFactorAuthenticate response";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                return null;
            }

            AuthCommandExecuting = false;

            return firstFactorAuthenticateResponse;
        }

        public UserAuthenticationViewModel(GrpcChannel grpcChannel)
        {

            var isValidObservable = this.WhenAnyValue(x => x.Login, x => x.Password,
                (login ,password) => !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password));


            AuthCommand = ReactiveCommand.CreateFromTask(AuthCommandOnExecute, isValidObservable);
            _grpcChannel = grpcChannel;

        }

        public InputTotpPasswordViewModel InputTotpPassword { get; set; }

    }
}
