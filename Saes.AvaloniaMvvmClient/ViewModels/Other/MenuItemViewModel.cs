using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Other
{
    public class MenuItemViewModel: ViewModelBase
    {
		private string _title;

        public event EventHandler<SubMenuItemViewModel> MenuItemClicked;

		public string Title
		{
			get { return _title; }
			set => this.RaiseAndSetIfChanged(ref _title, value);
		}

		private StreamGeometry _icon;

		public StreamGeometry Icon
		{
			get { return _icon; }
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }

        private ObservableCollection<SubMenuItemViewModel> _items;

        public ObservableCollection<SubMenuItemViewModel> Items
        {
            get { return _items; }
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public MenuItemViewModel(string title, string iconKey, ICollection<SubMenuItemViewModel> items = null)
        {
            Title = title;

            if(iconKey != null)
            {
                App.Current.TryFindResource(iconKey, out var icon);

                Icon = icon as StreamGeometry;
            }
            

            Items = new ObservableCollection<SubMenuItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;

            if (items != null)
            {
                foreach(var item in items)
                {
                    Items.Add(item);
                }
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var item = e.NewItems[0] as SubMenuItemViewModel;
                    item.SubMenuItemClicked += Item_SubMenuItemClicked;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var item2 = e.OldItems[0] as SubMenuItemViewModel;
                    item2.SubMenuItemClicked -= Item_SubMenuItemClicked;
                    break;
            }
        }

        private void OnMenuItemClicked(SubMenuItemViewModel subMenuItem)
        {
            MenuItemClicked?.Invoke(this, subMenuItem);
        }

        private void Item_SubMenuItemClicked(object sender, EventArgs e)
        {
            OnMenuItemClicked(sender as  SubMenuItemViewModel);
        }
    }
}
