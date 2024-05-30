using Avalonia.Reactive;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;

    public MainViewModel(AuthenticationMainViewModel authenticationMainViewModel)
    {
        //authenticationMainViewModel.AuthenticationCompleted += AuthenticationMainViewModel_AuthenticationCompleted;
        //ContentViewModel = authenticationMainViewModel;
        ContentViewModel = App.ServiceProvider.GetRequiredService<MainMenuViewModel>();
    }

    private void AuthenticationMainViewModel_AuthenticationCompleted(object sender, EventArgs e)
    {
        //ContentViewModel = new BigViewModel();
    }

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

}
