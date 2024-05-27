using Google.Protobuf.WellKnownTypes;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization
{
    public class SecondFactorAuthenticationViewModel: ViewModelBase
    {
        private string _totpPassword;
        private readonly IGrpcChannelFactory _grpcChannelFactory;

        public string TotpPassword
        {
            get { return _totpPassword; }
            set => this.RaiseAndSetIfChanged(ref _totpPassword, value);
        }

        private string _firstAuthToken;

        public string FirstAuthToken
        {
            get { return _firstAuthToken; }
            set => this.RaiseAndSetIfChanged(ref _firstAuthToken, value);
        }

        private bool _successCommandIsExecuting;

        public bool SuccessCommandIsExecuting
        {
            get { return _successCommandIsExecuting; }
            private set => this.RaiseAndSetIfChanged(ref _successCommandIsExecuting, value);
        }



        public ReactiveCommand<Unit, SecondFactorAuthenticateResponse> SuccessCommand { get; }

        public SecondFactorAuthenticationViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            var isValidObservable = this.WhenAnyValue(x => x.TotpPassword, x => !string.IsNullOrEmpty(x) && x.Replace(" ", "").Trim().Length == 6 && !SuccessCommandIsExecuting);
            SuccessCommand = ReactiveCommand.CreateFromTask(SuccessCommandOnExecute, isValidObservable);
            _grpcChannelFactory = grpcChannelFactory;
        }

        private async Task<SecondFactorAuthenticateResponse> SuccessCommandOnExecute()
        {
            SuccessCommandIsExecuting = true;

            var authService = new Authentication.AuthenticationClient(_grpcChannelFactory.CreateChannel());

            var request = new SecondFactorAuthenticateRequest
            {
                Token = _firstAuthToken,
                OtpPassword = _totpPassword,
                SendTime = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            SecondFactorAuthenticateResponse response;

            try
            {
                response = await authService.SecondFactorAuthenticateAsync(request);
            }
            catch (Exception ex)
            {
                return null;
            }

            SuccessCommandIsExecuting = false;

            return response;
        }
    }
}
