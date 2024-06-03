using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core.Enums;
using System;
using System.Reactive;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;

namespace Saes.AvaloniaMvvmClient.ViewModels;



public abstract class ViewModelFormBase <TDto, TDataRequest> : ViewModelBase
    where TDto : class, new()
    where TDataRequest: class, new()

{
    protected ViewModelFormBase()
    {
        FormCommand = ReactiveCommand.CreateFromTask(OnFormCommand);
    }
    public ReactiveCommand<Unit, Unit> FormCommand { get; set; }

    protected abstract Task OnFormCommand();

    protected FormMode _currentMode;

    public FormMode CurrentMode
    {
        get => _currentMode;
        set => this.RaiseAndSetIfChanged(ref _currentMode, value);
    }

    protected TDataRequest _dataRequest = new TDataRequest();
    public TDataRequest DataRequest
    {
        get => _dataRequest;
        set => this.RaiseAndSetIfChanged(ref _dataRequest, value);
    }


    protected TDto _dto = new TDto();
    public TDto Dto
    {
        get => _dto;
        protected set => this.RaiseAndSetIfChanged(ref _dto, value);
    }

    protected Action<TDto> _callback;
    public Action<TDto> Callback
    {
        get => _callback;
        set => this.RaiseAndSetIfChanged(ref _callback, value);
    }

    protected abstract void _Configure(TDto dto);

    public void Configure(FormMode formMode, Action<TDto> callback, TDto dto = null)
    {
        CurrentMode = formMode;
        Callback = callback;
        _Configure(dto);

    }
}
