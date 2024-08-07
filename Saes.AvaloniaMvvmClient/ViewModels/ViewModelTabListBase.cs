﻿using Avalonia.Input;
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
        protected ObservableCollection<TDto> _entities;
        protected bool _exportCommandIsExecuting;
        protected bool _searchCommandIsExecuting;
        protected TDto _selectedEntity;
        private TLookup _lookup = new TLookup();

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

        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ClearLookupCommand { get; set; }

        public ReactiveCommand<Unit, Unit> CopyCommand { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }

        public ReactiveCommand<Unit, Unit> DuplicateCommand { get; set; }

        public ReactiveCommand<Unit, Unit> EditCommand { get; set; }

        public ObservableCollection<TDto> Entities
        {
            get => _entities;
            set => this.RaiseAndSetIfChanged(ref _entities, value);
        }

        public ReactiveCommand<Unit, Unit> ExportCommand { get; set; }

        public bool ExportCommandIsExecuting
        {
            get => _exportCommandIsExecuting;
            set => this.RaiseAndSetIfChanged(ref _exportCommandIsExecuting, value);
        }

        public TLookup Lookup
        {
            get => _lookup;
            set => this.RaiseAndSetIfChanged(ref _lookup, value);
        }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; set; }

        public bool SearchCommandIsExecuting
        {
            get => _searchCommandIsExecuting;
            set => this.RaiseAndSetIfChanged(ref _searchCommandIsExecuting, value);
        }

        public ReactiveCommand<Unit, Unit> SeeCommand { get; set; }

        public TDto SelectedEntity
        {
            get => _selectedEntity;
            set => this.RaiseAndSetIfChanged(ref _selectedEntity, value);
        }
        //protected IObservable<bool> _searchCommandIsExecutingObservable
        //{
        //    get
        //    {
        //        return this.WhenAnyValue(x => x.SearchCommandIsExecuting, x => !x);
        //    }
        //}

        protected abstract Task _Export();

        protected abstract Task _Search();

        protected abstract Task OnAddCommand();

        protected abstract Task OnCopyCommand();

        protected abstract Task OnDeleteCommand();

        protected abstract Task OnEditCommand();

        protected virtual async Task OnExportCommand()
        {
            ExportCommandIsExecuting = true;

            await _Export();

            ExportCommandIsExecuting = false;
        }

        protected virtual async Task OnSearchCommand()
        {
            SearchCommandIsExecuting = TabIsLoading = true;

            await _Search();

            SearchCommandIsExecuting = TabIsLoading = false;
        }

        //protected IObservable<bool> _exportCommandIsExecutingObservable
        //{
        //    get
        //    {
        //        return this.WhenAnyValue(x => x.ExportCommandIsExecuting, x => !x);
        //    }
        //}
        protected abstract Task OnSeeCommand();
    }
}