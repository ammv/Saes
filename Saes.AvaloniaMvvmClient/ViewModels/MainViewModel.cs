using Avalonia.Reactive;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.Protos.Auth;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ISessionKeyService _sessionKeyService;

    public INavigationService NavigationService { get; }

    public MainViewModel(AuthenticationMainViewModel authenticationMainViewModel, INavigationService navigationService, ISessionKeyService sessionKeyService, IGrpcChannelFactory grpcChannelFactory, IUserService userService)
    {
        NavigationService = navigationService;
#if !DEBUG
        if (sessionKeyService.GetSessionKey() == null)
        {
            NavigationService.NavigateTo(authenticationMainViewModel);
            return;
        }

        try
        {
            userService.LoadRights();

            var authService = new Protos.Auth.Authentication.AuthenticationClient(grpcChannelFactory.CreateChannel());

            var request = new ValidateSessionKeyRequest { SessionKey = sessionKeyService.GetSessionKey() };

            var response = authService.ValidateSessionKey(request);

            if(response.Result)
            {
                NavigationService.NavigateTo(App.ServiceProvider.GetService<MainMenuViewModel>());
            }
            else
            {
                NavigationService.NavigateTo(authenticationMainViewModel);
            }

        }
        catch (Exception)
        {

            throw;
        }
        
        
#else
        NavigationService.NavigateTo(App.ServiceProvider.GetService<MainMenuViewModel>());
#endif
    }

}
