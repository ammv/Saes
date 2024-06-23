using DynamicData;
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
        public int SelectedIndex { get; set; }



        [Reactive]
        public ObservableCollection<T> Items { get; set; }

        public CollectionWithSelection(ICollection<T> items = null, T selected = default)
        {

            Items = new ObservableCollection<T>();
            if (items != null)
            {

                Items.AddRange(items);
            }
            Selected = selected;

            //ClearSelectedCommand = ReactiveCommand.Create(() => { Selected = default; SelectedIndex = -1; });
        }

        public CollectionWithSelection<T> Clone()
        {
            return new CollectionWithSelection<T>(Items, default);
        }

        public ReactiveCommand<Unit, Unit> ClearSelectedCommand { get; }
    }
}
