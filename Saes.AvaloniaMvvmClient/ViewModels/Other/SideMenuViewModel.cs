using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Other
{
    public class SideMenuViewModel: ViewModelBase, IDisposable
    {
        private ViewModelBase _content;

        private bool _isOpen;

        private ObservableCollection<MenuItemViewModel> _items;

        public SideMenuViewModel(ICollection<MenuItemViewModel> items = null)
        {
            TriggerPaneCommand = ReactiveCommand.Create(() =>
            {
                IsOpen = !IsOpen;
            });

            Items = new ObservableCollection<MenuItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;

            if (items != null)
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
        }

        public event EventHandler<SubMenuItemViewModel> SideMenuItemClicked;
        public ViewModelBase Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        public bool IsOpen
        {
			get { return _isOpen; }
			set { this.RaiseAndSetIfChanged(ref _isOpen, value); }
		}
        public ObservableCollection<MenuItemViewModel> Items
        {
            get { return _items; }
            set { this.RaiseAndSetIfChanged(ref _items, value); }
        }
        public ReactiveCommand<Unit, Unit> TriggerPaneCommand { get; }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0] as MenuItemViewModel;
                    newItem.MenuItemClicked += NewItem_MenuItemClicked;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as MenuItemViewModel;
                    oldItem.MenuItemClicked -= NewItem_MenuItemClicked;
                    oldItem.Dispose();
                    break;
            }
        }

        private void NewItem_MenuItemClicked(object sender, SubMenuItemViewModel e)
        {
            OnSideMenuItemClicked(e);
        }
        private void OnSideMenuItemClicked(SubMenuItemViewModel subMenuItem)
        {
            SideMenuItemClicked?.Invoke(this, subMenuItem);
        }

        public void Dispose()
        {
            foreach (var item in _items)
            {
                item.MenuItemClicked -= NewItem_MenuItemClicked;
                item.Dispose();
            }
            _items.CollectionChanged -= Items_CollectionChanged;
            Items = null;
            Content = null;
            Debug.WriteLine("SideMenuViewModel disposed");

        }
    }
}
