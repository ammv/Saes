using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authentication
{
    public class AuthenticationMainViewModel : ViewModelBase
    {
        private readonly ISessionKeyService _sessionKeyService;
        private readonly INavigationServiceFactory _navigationServiceFactory;
        private readonly IUserService _userService;
        private readonly IWindowTitleService _windowTitleService;
        private readonly FirstFactorAuthenticationViewModel _firstFactorAuthenticationViewModel;
        private SecondFactorAuthenticationViewModel _secondFactorAuthenticationViewModel;

        public ReactiveCommand<Unit, Unit> LoadedCommand { get; }
        [Reactive]
        private bool LoadingStarted { get; set; }

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
            _navigationServiceFactory.Singleton.NavigateTo(App.ServiceProvider.GetService<MainMenuViewModel>());
        }

        public AuthenticationMainViewModel(FirstFactorAuthenticationViewModel firstFactorAuthenticationViewModel, ISessionKeyService sessionKeyService, INavigationServiceFactory navigationServiceFactory, IUserService userService, IWindowTitleService windowTitleService)
        {
            _firstFactorAuthenticationViewModel = firstFactorAuthenticationViewModel;
            _sessionKeyService = sessionKeyService;
            _navigationServiceFactory = navigationServiceFactory;
            _userService = userService;
            _windowTitleService = windowTitleService;

            LoadedCommand = ReactiveCommand.Create(OnLoadedCommand, this.WhenAnyValue( x => x.LoadingStarted, x => !x));

            DialogMode = false;
        }
        public void OnLoadedCommand()
        {
            if (LoadingStarted) return;
            LoadingStarted = true;

            _windowTitleService.TitleFormat = "{appName}";
            NavigationService = _navigationServiceFactory.Create();
            _firstFactorAuthenticationViewModel.AuthCommand.Subscribe(FirstFactorCommandOnExecute);
            NavigationService.NavigateTo(_firstFactorAuthenticationViewModel);

        }

        private void FirstFactorCommandOnExecute(FirstFactorAuthenticateResponse response)
        {
            if (response == null) return;

            if (!response.Has2FA)
            {
                _sessionKeyService.SaveSessionKey(response.SessionKey);
                OnAuthenticationCompleted();
                return;
            }

            if (_secondFactorAuthenticationViewModel == null)
            {
                _secondFactorAuthenticationViewModel = App.ServiceProvider.GetService<SecondFactorAuthenticationViewModel>();

                _secondFactorAuthenticationViewModel.FirstAuthToken = response.Token;

                _secondFactorAuthenticationViewModel.SuccessCommand.Subscribe(SecondFactorCommandOnExecute);
            }

            NavigationService.NavigateTo(_secondFactorAuthenticationViewModel);

        }

        private int _badOtpAttempts = 0;
        private const int _maxBadOtpAttempts = 3;


        private void SecondFactorCommandOnExecute(SecondFactorAuthenticateResponse response)
        {
            if (response == null)
            {
                if (++_badOtpAttempts == _maxBadOtpAttempts)
                {
                    NavigationService.NavigateTo(_firstFactorAuthenticationViewModel);
                    _badOtpAttempts = 0;
                }
                return;
            }

            _sessionKeyService.SaveSessionKey(response.SessionKey);
            OnAuthenticationCompleted();
        }

        [Reactive]
        public INavigationService NavigationService { get; private set; }
    }
}
