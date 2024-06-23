using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authentication
{
    public class AuthenticationMainViewModel: ViewModelBase
    {
        private readonly IGrpcChannelFactory _grpcChannelFactory;
        private readonly ISessionKeyService _sessionKeyService;
        private readonly IUserService _userService;
        private readonly FirstFactorAuthenticationViewModel _firstFactorAuthenticationViewModel;
        private SecondFactorAuthenticationViewModel _secondFactorAuthenticationViewModel;

        [Reactive]
        public bool DialogMode { get; set; }

        [Reactive]
        public Action CompleteCallback { get; set; }

        protected virtual void OnAuthenticationCompleted()
        {
            _userService.LoadRights();
            if (DialogMode)
            {
                CompleteCallback?.Invoke();
                return;
            }
            NavigationService.NavigateTo(App.ServiceProvider.GetService<MainMenuViewModel>());
        }
        [Reactive]
        public ViewModelBase Content { get; set; }

        public AuthenticationMainViewModel(FirstFactorAuthenticationViewModel firstFactorAuthenticationViewModel, ISessionKeyService sessionKeyService, INavigationService navigationService, IUserService userService)
        {
            _firstFactorAuthenticationViewModel = firstFactorAuthenticationViewModel;
            _sessionKeyService = sessionKeyService;
            NavigationService = navigationService;
            _userService = userService;
            _firstFactorAuthenticationViewModel.AuthCommand.Subscribe(FirstFactorCommandOnExecute);

            Content = _firstFactorAuthenticationViewModel;
            DialogMode = false;
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

            Content = _secondFactorAuthenticationViewModel;

        }
        
        private void SecondFactorCommandOnExecute(SecondFactorAuthenticateResponse response)
        {
            if (response == null)
            {
                Content = _firstFactorAuthenticationViewModel;
                return;
            }

            _sessionKeyService.SaveSessionKey(response.SessionKey);
            OnAuthenticationCompleted();
        }

        public INavigationService NavigationService { get; }
    }
}
