using ReactiveUI;
using Saes.AvaloniaMvvmClient.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public class AddItemViewModel: ViewModelBase
    {
        private string _description = string.Empty;

        public ReactiveCommand<Unit, ToDoItem> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public AddItemViewModel()
        {
            
            var isValidObservable = this.WhenAnyValue(
                x => x.Description,
                x => !string.IsNullOrWhiteSpace(x));

            var isTrueObservable = this.WhenAny(
                _ => _,
                x => true
                );

            OkCommand = ReactiveCommand.Create(
                () => new ToDoItem { Description = Description }, isValidObservable);
            CancelCommand = ReactiveCommand.Create(
                () => { }, isTrueObservable);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

    }
}
