using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication.User;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.AvaloniaMvvmClient.Views.Authentication.User;
using Saes.Protos;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowManager.Add(this);
        Closed += (s, e) =>
        {
            WindowManager.Remove(this);
            var vm = DataContext as MainMenuViewModel;
        };
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CenterWindow();
        var vm = DataContext as MainViewModel;
        vm.NavigationServiceFactory.Created += (s, e) => e.Navigated += (s,e) => CenterWindow();
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
