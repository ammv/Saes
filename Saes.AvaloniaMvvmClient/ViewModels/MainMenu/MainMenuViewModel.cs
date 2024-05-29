using Avalonia;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.ViewModels.Administration.User;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.Other;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels.MainMenu
{
    public class MainMenuViewModel: ViewModelBase
    {
        public TabStripViewModel TabStrip { get; }
        public SideMenuViewModel Menu { get; }

        public MainMenuViewModel()
        {
            TabStrip = new TabStripViewModel(new ObservableCollection<TabStripItemViewModel>());

            Menu = new SideMenuViewModel(
                new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel("Администрирование", "home_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Пользователи", "people_team_regular", typeof(UserListViewModel)),
                        new SubMenuItemViewModel("Sub item 2", "data_sunburst_regular", null),
                        new SubMenuItemViewModel("Sub item 3", "data_sunburst_regular", null),
                    }),
                    new MenuItemViewModel("Menu item 2", "people_community_add_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Sub item 1", "data_sunburst_regular", null),
                        new SubMenuItemViewModel("Sub item 2", "data_sunburst_regular", null),
                        new SubMenuItemViewModel("Sub item 3", "data_sunburst_regular", null),
                    })
                }
            );

            Menu.SideMenuItemClicked += Menu_MenuButtonClicked;
        }

        private void Menu_MenuButtonClicked(object sender, SubMenuItemViewModel e)
        {
            if(e.ViewModelType == null)
            {
                return;
            }
            TabStrip.Tabs.Add(new TabStripItemViewModel
            {
                Title = e.Title,
                Content = App.ServiceProvider.GetService(e.ViewModelType) as ViewModelCloseableBase
            });
        }
    }
}
