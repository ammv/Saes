using Avalonia.Reactive;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{


    public INavigationService NavigationService { get; }

    public MainViewModel(AuthenticationMainViewModel authenticationMainViewModel, INavigationService navigationService)
    {
        NavigationService = navigationService;
#if !DEBUG
        NavigationService.NavigateTo(authenticationMainViewModel);
#else
        NavigationService.NavigateTo(App.ServiceProvider.GetService<MainMenuViewModel>());
#endif
    }

}
