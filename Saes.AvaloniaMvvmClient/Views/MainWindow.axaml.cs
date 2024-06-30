using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication.User;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.AvaloniaMvvmClient.Views.Authentication.User;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Views;

public partial class MainWindow : Window
{
    private List<INavigationService> _navigationServices = new List<INavigationService>();
    public MainWindow()
    {
        InitializeComponent();
        WindowManager.Add(this);
        Closed += (s, e) =>
        {

            WindowManager.Remove(this);
            foreach(var nav in _navigationServices)
            {
                nav.Navigated -= NavigationService_Navigating;
                nav.Navigating -= NavigationService_Navigating;
            }
            (DataContext as MainViewModel).NavigationServiceFactory.Created -= NavigationServiceFactory_Created;
        };
        Loaded += MainWindow_Loaded;
        CenterWindow();
    }

    private void MainWindow_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = DataContext as MainViewModel;

        vm.NavigationServiceFactory.Created += NavigationServiceFactory_Created;
    }

    private void NavigationServiceFactory_Created(object sender, INavigationService e)
    {
        e.Navigating += NavigationService_Navigating;
    }

    private void NavigationService_Navigating(object sender, ViewModelBase e)
    {
        CenterWindow();
    }

    private void CenterWindow()
    {
        var screen = Screens.ScreenFromWindow(this);
        var screenWidth = screen.WorkingArea.Width;
        var screenHeight = screen.WorkingArea.Height;

        var x = (int)(screenWidth - this.Bounds.Size.Width) / 2;
        var y = (int)(screenHeight - this.Bounds.Size.Height) / 2;

        this.Position = new Avalonia.PixelPoint(x, y);
    }
}
