using Avalonia.Input;
using ReactiveUI;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public abstract class ViewModelTabListBase<TDto, TLookup> : ViewModelTabBase
        where TLookup : class, new()
        where TDto : class, new()
    {

        private TLookup _lookup = new TLookup();
        public TLookup Lookup
        {
            get => _lookup;
            set => this.RaiseAndSetIfChanged(ref _lookup, value);
        }

        protected TDto _selectedEntity;
        public TDto SelectedEntity
        {
            get => _selectedEntity;
            set => this.RaiseAndSetIfChanged(ref _selectedEntity, value);
        }

        protected ObservableCollection<TDto> _entities;

        public ObservableCollection<TDto> Entities
        {
            get => _entities;
            set => this.RaiseAndSetIfChanged(ref _entities, value);
        }

        protected bool _searchCommandIsExecuting;
        public bool SearchCommandIsExecuting
        {
            get => _searchCommandIsExecuting;
            set => this.RaiseAndSetIfChanged(ref _searchCommandIsExecuting, value);
        }

        protected bool _exportCommandIsExecuting;
        public bool ExportCommandIsExecuting
        {
            get => _exportCommandIsExecuting;
            set => this.RaiseAndSetIfChanged(ref _exportCommandIsExecuting, value);
        }

        protected IObservable<bool> _tabIsLoadingObservable
        {
            get
            {
                return this.WhenAnyValue(x => x.TabIsLoading, x => !x);
            }
        }

        protected IObservable<bool> _searchCommandIsExecutingObservable
        {
            get
            {
                return this.WhenAnyValue(x => x.SearchCommandIsExecuting, x => !x);
            }
        }

        protected IObservable<bool> _exportCommandIsExecutingObservable
        {
            get
            {
                return this.WhenAnyValue(x => x.ExportCommandIsExecuting, x => !x);
            }
        }

        protected ViewModelTabListBase()
        {
            SearchCommand = ReactiveCommand.CreateFromTask(OnSearchCommand, this.WhenAnyValue(
    x => x.TabIsLoading,
    x => x.SearchCommandIsExecuting,
    (tabIsLoading, searchCommandIsExecuting) => !tabIsLoading && !searchCommandIsExecuting));
            ExportCommand = ReactiveCommand.CreateFromTask(OnSearchCommand, this.WhenAnyValue(
    x => x.TabIsLoading,
    x => x.ExportCommandIsExecuting,
    (tabIsLoading, exportCommandIsExecuting) => !tabIsLoading && !exportCommandIsExecuting));
            ClearLookupCommand = ReactiveCommand.Create(() => { Lookup = new TLookup(); });

            SeeCommand = ReactiveCommand.CreateFromTask(OnSeeCommand);
            AddCommand = ReactiveCommand.CreateFromTask(OnAddCommand);
            DeleteCommand = ReactiveCommand.CreateFromTask(OnDeleteCommand);
            EditCommand = ReactiveCommand.CreateFromTask(OnEditCommand);
            CopyCommand = ReactiveCommand.CreateFromTask(OnCopyCommand);
        }

        protected abstract Task OnSeeCommand();
        protected abstract Task OnAddCommand();
        protected abstract Task OnDeleteCommand();
        protected abstract Task OnEditCommand();
        protected abstract Task OnCopyCommand();

        public ReactiveCommand<Unit, Unit> SeeCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> EditCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DuplicateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ClearLookupCommand { get; set; }

        protected bool _tabIsLoading;
        public bool TabIsLoading
        {
            get => _tabIsLoading;
            set => this.RaiseAndSetIfChanged(ref _tabIsLoading, value);
        }

        protected virtual async Task OnSearchCommand()
        {
            SearchCommandIsExecuting = TabIsLoading = true;

            await _Search();

            SearchCommandIsExecuting = TabIsLoading = false;
        }

        protected virtual async Task OnExportCommand()
        {
            ExportCommandIsExecuting = true;

            await _Export();

            ExportCommandIsExecuting = false;
        }

        public bool TabIsLoaded { get; private set; } = false;

        protected abstract Task _Search();
        protected abstract Task _Export();
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
