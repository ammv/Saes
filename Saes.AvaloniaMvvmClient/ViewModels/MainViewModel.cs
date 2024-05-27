using Avalonia.Reactive;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;



    public MainViewModel(AuthenticationMainViewModel authenticationMainViewModel)
    {
        authenticationMainViewModel.AuthenticationCompleted += AuthenticationMainViewModel_AuthenticationCompleted;
        ContentViewModel = authenticationMainViewModel;
    }

    private void AuthenticationMainViewModel_AuthenticationCompleted(object sender, EventArgs e)
    {
        ContentViewModel = null;
    }

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

}
