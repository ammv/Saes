using Avalonia.Reactive;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;



    public MainViewModel()
    {
        
        _contentViewModel = null;
    }

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

}
