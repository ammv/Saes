using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization
{
    public class AuthenticationMainViewModel: ViewModelBase
    {
        private readonly IGrpcChannelFactory _grpcChannelFactory;
        private readonly FirstFactorAuthenticationViewModel _firstFactorAuthenticationViewModel;
        private readonly ISessionKeyService _sessionKeyService;
        private SecondFactorAuthenticationViewModel _secondFactorAuthenticationViewModel;

        public event EventHandler AuthenticationCompleted;

        protected virtual void OnAuthenticationCompleted()
        {
            AuthenticationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public AuthenticationMainViewModel(FirstFactorAuthenticationViewModel firstFactorAuthenticationViewModel, ISessionKeyService sessionKeyService)
        {
            _firstFactorAuthenticationViewModel = firstFactorAuthenticationViewModel;
            _sessionKeyService = sessionKeyService;
            _firstFactorAuthenticationViewModel.AuthCommand.Subscribe(FirstFactorCommandOnExecute);

            ContentViewModel = _firstFactorAuthenticationViewModel;
        }

        private void FirstFactorCommandOnExecute(FirstFactorAuthenticateResponse response)
        {
            if (response == null) return;

            if(!response.Has2FA)
            {
                _sessionKeyService.SaveSessionKey(response.SessionKey);
                OnAuthenticationCompleted();
                return;
            }

            if(_secondFactorAuthenticationViewModel == null)
            {
                _secondFactorAuthenticationViewModel = App.ServiceProvider.GetService<SecondFactorAuthenticationViewModel>();

                _secondFactorAuthenticationViewModel.FirstAuthToken = response.Token;

                _secondFactorAuthenticationViewModel.SuccessCommand.Subscribe(SecondFactorCommandOnExecute);
            } 

            ContentViewModel = _secondFactorAuthenticationViewModel;

        }

        private void SecondFactorCommandOnExecute(SecondFactorAuthenticateResponse response)
        {
            if (response == null)
            {
                ContentViewModel = _firstFactorAuthenticationViewModel;
            }

            _sessionKeyService.SaveSessionKey(response.SessionKey);
            OnAuthenticationCompleted();
        }

        private ViewModelBase _contentViewModel;

        public ViewModelBase ContentViewModel
        {
            get { return _contentViewModel; }
            private set { this.RaiseAndSetIfChanged(ref _contentViewModel, value); }
        }

    }
}
