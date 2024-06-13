using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core.Enums;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;

namespace Saes.AvaloniaMvvmClient.ViewModels;



public abstract class ViewModelFormBase <TDto, TDataRequest> : ViewModelBase
    where TDto : class, new()
    where TDataRequest: class, new()

{


    private bool _formCommandIsExecuting;
    public bool FormCommandIsExecuting
    {
        get => _formCommandIsExecuting;
        set => this.RaiseAndSetIfChanged(ref _formCommandIsExecuting, value);
    }

    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    protected abstract bool Validate();

    protected ViewModelFormBase()
    {
    
        FormCommand = ReactiveCommand.CreateFromTask(OnFormCommand, this.WhenAnyValue(x => x.FormCommandIsExecuting, x => x.TabIsLoading, (x,y) => !x && !y));
    }
    public ReactiveCommand<Unit, Unit> FormCommand { get; set; }

    protected async Task OnFormCommand()
    {
        FormCommandIsExecuting = true;

        await _OnPreFormCommand();

        switch(_currentMode)
        {
            case FormMode.See:
                await _OnSee();
                break;
            case FormMode.Edit:
                await _OnEdit();
                break;
            case FormMode.Add:
                await _OnAdd();
                break;
        }

        FormCommandIsExecuting = false;
    }

    protected abstract Task _OnPreFormCommand();

    protected FormMode _currentMode;

    public FormMode CurrentMode
    {
        get => _currentMode;
        set 
        {
            this.RaiseAndSetIfChanged(ref _currentMode, value);
            _ConfigureTitle();
        }
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

    protected abstract TDataRequest _Configure(TDto dto);

    public void Configure(FormMode formMode, Action<TDto> callback, TDto dto = null)
    {
        CurrentMode = formMode;
        Callback = callback;
        Dto = dto;
    }

    protected abstract Task _OnEdit();
    protected abstract Task _OnSee();
    protected abstract Task _OnAdd();

    private bool _tabIsLoading;
    public bool TabIsLoading
    {
        get => _tabIsLoading;
        set => this.RaiseAndSetIfChanged(ref _tabIsLoading, value);
    }

    protected abstract void _ConfigureTitle();

    protected abstract Task _Loaded();
    public virtual async void Loaded()
    {
        TabIsLoading = true;

        await _Loaded();

        DataRequest = _Configure(Dto);

        TabIsLoading = false;
    }
}
