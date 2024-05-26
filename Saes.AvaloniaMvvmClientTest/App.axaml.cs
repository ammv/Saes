using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Grpc.Net.Client;
using Saes.AvaloniaMvvmClientTest.ViewModels;
using Saes.AvaloniaMvvmClientTest.Views;

namespace Saes.AvaloniaMvvmClientTest;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7231");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(channel)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(channel)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
