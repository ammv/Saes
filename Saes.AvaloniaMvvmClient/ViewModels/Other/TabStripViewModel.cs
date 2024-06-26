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
        public event EventHandler<TabStripItemViewModel> SelectedTabChanging;
        public event EventHandler<TabStripItemViewModel> SelectedTabChanged;

        public event EventHandler<int> SelectedIndexChanging;
        public event EventHandler<int> SelectedIndexChanged;

        //public event EventHandler<TabStripItemViewModel> TabAdding;
        //public event EventHandler<TabStripItemViewModel> TabAdded;

        public event EventHandler<TabStripItemViewModel> TabRemoving;
        public event EventHandler<TabStripItemViewModel> TabRemoved;

        //public event EventHandler<TabStripItemViewModel> TabClosing;
        //public event EventHandler<TabStripItemViewModel> TabClosed;

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
            set
            {
                SelectedTabChanging?.Invoke(this, value);
                this.RaiseAndSetIfChanged(ref _selectedTab, value);
                SelectedTabChanged?.Invoke(this, _selectedTab);
            } 
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                SelectedIndexChanging?.Invoke(this, value);
                this.RaiseAndSetIfChanged(ref _selectedIndex, value);
                SelectedIndexChanged?.Invoke(this, _selectedIndex);
            } 
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
                    SelectedTab = Tabs.Last();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as TabStripItemViewModel;
                    oldItem.Closed -= TabStripItem_Closed;
                    break;
            }
        }

        private void TabStripItem_Closed(object sender, EventArgs e)
        {
            var vm = sender as TabStripItemViewModel;
            TabRemoving?.Invoke(this, vm);
            Tabs.Remove(vm);
            TabRemoved?.Invoke(this, vm);

        }
    }
}
