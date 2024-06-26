using Avalonia.Input;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected IObservable<bool> _tabIsLoadingObservable
        {
            get
            {
                return this.WhenAnyValue(x => x.TabIsLoading, x => !x);
            }
        }

        protected bool _tabIsLoading;
        public bool TabIsLoading
        {
            get => _tabIsLoading;
            set => this.RaiseAndSetIfChanged(ref _tabIsLoading, value);
        }

        public bool TabIsLoaded { get; private set; } = false;

        protected abstract Task _Loaded();
        public virtual async void Loaded()
        {
            if (TabIsLoaded) return;

            TabIsLoading = true;

            await _Loaded();

            TabIsLoading = false;

            TabIsLoaded = true;
        }
    }
}
