using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Injections;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.Views;
using System;
using System.Collections.Generic;

namespace Saes.AvaloniaMvvmClient;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        //var grpcStatusMessageBus = new MessageBus();
        //collection.AddSingleton<IGrpcStatusMessageBus>(,);

        var excelExporterConfig = new ExcelExporterConfig();
        ConfigureExcelExporterConfig.Configure(excelExporterConfig);

        collection.AddSingleton(excelExporterConfig);

        collection.AddCommonServices();
        collection.AddGrpcServices();
        collection.AddMainViewModels();
        collection.AddAuthenticationViewModels();
        collection.AddElectricitySignsViewModels();
        collection.AddAuditViewModels();
        collection.AddHumanResourcesViewModels();
        collection.AddAuthorizationViewModels();
        collection.AddOfficeViewModels();
        collection.AddPersonViewModels();
        collection.AddOtherViewModels();

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        ServiceProvider = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if(e.ExceptionObject is Exception ex)
        {
            await MessageBoxHelper.Exception("Неизвестная ошибка", $"Произошла неизвестная ошибка:\n{ex.Message}");
        }
        else
        {
            await MessageBoxHelper.Exception("Неизвестная ошибка", $"Произошла неизвестная ошибка");
        }
    }
}
