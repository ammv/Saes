using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public class CollectionWithSelection<T> : ReactiveObject
    {
        [Reactive]
        public T Selected { get; set; }


        [Reactive]
        public ICollection<T> Items {get; set;}

        public CollectionWithSelection(ICollection<T> items = null, T selected = default)
        {

            Items = items ?? new ObservableCollection<T>();
            Selected = selected;

            ClearSelectedCommand = ReactiveCommand.Create(() => { Selected = default;  });
        }

        public CollectionWithSelection<T> Clone()
        {
            return new CollectionWithSelection<T>(Items, default);
        }

        public ReactiveCommand<Unit, Unit> ClearSelectedCommand { get; }
    }
}
