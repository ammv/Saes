using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ReactiveUI;
using Saes.Protos.Auth;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClientTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenNewWindowCommand;
    public MainViewModel()
    {
        OpenNewWindowCommand = ReactiveCommand.Create(OnOpenNewWindowCommand);
    }

    private void OnOpenNewWindowCommand()
    {

    }

    private ViewModelBase _contentViewModel;
    public ViewModelBase ContentViewModel
    {
        get { return _contentViewModel; }
        private set { this.RaiseAndSetIfChanged(ref _contentViewModel, value); }
    }

}
