using Avalonia.Input;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public abstract class ViewModelTabBase: ViewModelBase
    {
        protected string _tabTitle;
        public string TabTitle
        {
            get => _tabTitle;
            set => this.RaiseAndSetIfChanged(ref _tabTitle, value);
        }
        public virtual async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected ViewModelTabBase()
        {
            LoadedCommand = ReactiveCommand.CreateFromTask(OnLoadedCommand, this.WhenAnyValue(x => x.LoadingStarted, x => !x));
        }

        public ReactiveCommand<Unit, Unit> LoadedCommand { get; protected set; }

        //protected IObservable<bool> _tabIsLoadingObservable
        //{
        //    get
        //    {
        //        return this.WhenAnyValue(x => x.TabIsLoading, x => !x);
        //    }
        //}

        protected bool _tabIsLoading;
        public bool TabIsLoading
        {
            get => _tabIsLoading;
            set => this.RaiseAndSetIfChanged(ref _tabIsLoading, value);
        }

        private bool _loadingStarted = false;
        public bool LoadingStarted
        {
            get => _loadingStarted;
            set => this.RaiseAndSetIfChanged(ref _loadingStarted, value);
        }

        protected abstract Task _Loaded();
        public virtual async Task OnLoadedCommand()
        {
            if (LoadingStarted) return;
            LoadingStarted = true;

            TabIsLoading = true;

            await _Loaded();

            TabIsLoading = false;  
        }
    }
}
