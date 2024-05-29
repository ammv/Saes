using ReactiveUI;
using Saes.AvaloniaMvvmClient.ViewModels.Other;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Other
{
    public class TabStripViewModel: ViewModelBase
    {
        private ObservableCollection<TabStripItemViewModel> _tabs;

        public ObservableCollection<TabStripItemViewModel> Tabs
        {
            get { return _tabs; }
            set => this.RaiseAndSetIfChanged(ref _tabs, value);
        }

        private TabStripItemViewModel _selectedTab;

        public TabStripItemViewModel SelectedTab
        {
            get { return _selectedTab; }
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }


        public TabStripViewModel(ICollection<TabStripItemViewModel> tabs = null)
        {
            Tabs = new ObservableCollection<TabStripItemViewModel>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;

            if (tabs == null) return;

            foreach(var tab in tabs)
            {
                Tabs.Add(tab);
            }
        }

        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0] as TabStripItemViewModel;
                    newItem.Closed += TabStripItem_Closed;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as TabStripItemViewModel;
                    oldItem.Closed -= TabStripItem_Closed;
                    break;
            }
        }

        private void TabStripItem_Closed(object sender, EventArgs e)
        {
            Tabs.Remove(sender as TabStripItemViewModel);
        }
    }
}
