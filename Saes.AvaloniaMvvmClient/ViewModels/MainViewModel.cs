using Avalonia.Controls;
using Avalonia.Reactive;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.AvaloniaMvvmClient.ViewModels.Other;
using Saes.Protos.Auth;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive]
    public TabStripViewModel TabStripViewModel { get; private set; }
    private readonly IServiceProvider _serviceProvider;
    
    public static Func<Window, bool> Selector = (x) => x.DataContext.GetType() == typeof(MainViewModel);

    public INavigationServiceFactory NavigationServiceFactory { get; }
    public IWindowTitleService WindowTitleService { get; }
    public IWindowStateService WindowStateService { get; }

    public MainViewModel(TabStripViewModel tabStripViewModel,INavigationServiceFactory navigationServiceFactory, IServiceProvider serviceProvider, IWindowTitleService windowTitleService, IWindowStateService windowStateService)
    {
        TabStripViewModel = tabStripViewModel;
        NavigationServiceFactory = navigationServiceFactory;
        _serviceProvider = serviceProvider;
        WindowTitleService = windowTitleService;
        WindowStateService = windowStateService;
    }

    public void Loaded()
    {
        WindowStateService.State = WindowState.Normal;
        WindowTitleService.AddOrUpdate("appName", "Система учёта электронных подписей");
        WindowTitleService.TitleFormat = "{appName}";

        NavigationServiceFactory.Singleton.NavigateTo(_serviceProvider.GetService<LoadingViewModel>());
    }

}
