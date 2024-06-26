using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core.Enums;
using Saes.AvaloniaMvvmClient.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public abstract class ViewModelFormBase <TDto, TDataRequest> : ViewModelCloseableBase
    where TDto : class, new()
    where TDataRequest: class, new()

{
    private int? _additionalInitialHashCode;

    private int _dataRequestInitialHashCode;
    protected bool FormDataChanged => (_dataRequest.GetHashCode() != _dataRequestInitialHashCode) || (_additionalInitialHashCode != null ? GetAdditionalHashCode() != _additionalInitialHashCode: false);

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
    
        FormCommand = ReactiveCommand.CreateFromTask(OnFormCommand, this.WhenAnyValue(x => x.FormCommandIsExecuting, x => x.FormIsLoading, (x,y) => !x && !y));
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

    protected virtual Task _OnPreFormCommand()
    {
        return Task.CompletedTask;
    }

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

    protected abstract TDataRequest _ConfigureDataRequest(TDto dto);

    public void Configure(FormMode formMode, Action<TDto> callback, TDto dto = null)
    {
        CurrentMode = formMode;
        Callback = callback;
        Dto = dto;
    }

    protected abstract Task _OnEdit();
    protected abstract Task _OnSee();
    protected abstract Task _OnAdd();

    [Reactive]
    public bool FormIsLoading { get; private set; }

    protected virtual int? GetAdditionalHashCode()
    {
        return null;
    }

    protected abstract void _ConfigureTitle();

    protected abstract Task _Loaded();
    public virtual async void Loaded()
    {
        FormIsLoading = true;

        await _Loaded();

        // Я перенес сюда DataRequest из-за того что загружаемые данные перебивали привязки в DataRequest и его поля принимали значение Null

        DataRequest = _ConfigureDataRequest(Dto);

        _dataRequestInitialHashCode = _dataRequest.GetHashCode();
        _additionalInitialHashCode = GetAdditionalHashCode();

        FormIsLoading = false;
    }

    protected override async Task OnClosingCommand(WindowClosingEventArgs closingEventArgs)
    {
        if(!IsForceClose && FormDataChanged)
        {
            closingEventArgs.Cancel = true;

            var result = await MessageBoxHelper.Question("Предупреждение", "У вас есть не сохраненные изменения, в случае закрытия окна они пропадут. Вы уверены что хотите закрыть окно?", isWarning: true);
            if(result)
            {
                Close();
            }
        }
    }
}
