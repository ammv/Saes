using Avalonia.Controls;
using Avalonia.Reactive;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.Protos.Auth;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AuthenticationMainViewModel _authenticationMainViewModel;
    private readonly IServiceProvider _serviceProvider;
    
    public static Func<Window, bool> Selector = (x) => x.DataContext.GetType() == typeof(MainViewModel);

    public INavigationService NavigationService { get; }

    public MainViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    {
        NavigationService = navigationService;
        _serviceProvider = serviceProvider;
    }

    public async void Loaded()
    {
        NavigationService.NavigateTo(_serviceProvider.GetService<LoadingViewModel>());
    }

}
