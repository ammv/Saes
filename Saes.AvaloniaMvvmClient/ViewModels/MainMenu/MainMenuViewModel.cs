using Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
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

        [Reactive]
        public StatusData Status { get; set; }

        public MainMenuViewModel()
        {
            TabStrip = new TabStripViewModel(new ObservableCollection<TabStripItemViewModel>());

            Menu = new SideMenuViewModel(
                new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel("Аудит", "send_logging_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Данные таблиц", "table_freeze_regular", null),
                        new SubMenuItemViewModel("Данные столбцов таблиц", "column_triple_regular", null),
                        new SubMenuItemViewModel("Логи аутенфикаций", "person_arrow_right_regular", null),
                        new SubMenuItemViewModel("Логи ошибок", "error_circle_regular", null),
                        new SubMenuItemViewModel("Логи действий", "book_database_regular", null),
                        new SubMenuItemViewModel("Логи изменений", "book_database_regular", null),
                    }),
                    new MenuItemViewModel("Авторизация", "people_community_add_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Роли пользователей", "book_regular", null),
                        new SubMenuItemViewModel("Группы прав", "book_regular", null),
                        new SubMenuItemViewModel("Права", "book_regular", null),
                        new SubMenuItemViewModel("Права ролей", "book_regular", null),
                        new SubMenuItemViewModel("Сессии", "share_screen_regular", null),
                        
                    }),
                    new MenuItemViewModel("Аутенфикация", "fingerprint_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Пользователи", "people_team_regular", typeof(UserListViewModel)),
                    }),
                    new MenuItemViewModel("Человечские ресурсы", "people_team_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Типы бизнес-сущностей", "book_regular", null),
                         new SubMenuItemViewModel("Бизнес-сущности", "people_team_regular", null),
                         new SubMenuItemViewModel("Организации", "briefcase_regular", null),
                         new SubMenuItemViewModel("Контакты организаций", "call_regular", null),
                         new SubMenuItemViewModel("Должности сотрудников", "book_regular", null),
                         new SubMenuItemViewModel("Сотрудники", "people_team_regular", null),
                    }),
                    new MenuItemViewModel("Личная информация", "contact_card_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Типы контактов", "book_regular", null),
                         
                    }),
                    new MenuItemViewModel("Электронные подписи", "signature_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Журнал технический (аппаратный) ", "notebook_regular", null),
                         new SubMenuItemViewModel("Журнал поэкземплярного учета СКЗИ для органа криптографической защиты", "notebook_regular", null),
                         new SubMenuItemViewModel("Журнал поэкземплярного учета СКЗИ, эксплуатационной и технической документации к ним, ключевых документов (для обладателя конфиденциальной информации) ", "notebook_regular", null),
                         new SubMenuItemViewModel("Субъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которым была разослана информация", "people_community_regular", null),
                         new SubMenuItemViewModel("Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, произведших подключение (установку)",
                         "people_community_regular", null
                         ),
                         new SubMenuItemViewModel("Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, производивших изъятие (уничтожение)", "people_community_regular", null),
                         new SubMenuItemViewModel("Номера аппаратных средств, в которые установлены или к которым подключены СКЗИ", "notebook_regular", null),
                         new SubMenuItemViewModel("Типы ключевых документов", "document_page_top_right_regular", null),
                         new SubMenuItemViewModel("Типы ключевых носителей", "usb_stick_regular", null),
                         new SubMenuItemViewModel("Ключевые носители", "key_regular", null),

                    }),
                    new MenuItemViewModel("Офис", "office_chair_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Аппаратура", "phone_laptop_regular", null),
                    }),
                    new MenuItemViewModel("Прочее", "book_question_mark_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Файлы", "document_regular", null),
                         new SubMenuItemViewModel("Адреса", "location_regular", null),
                    }),
                }
            );

            Menu.SideMenuItemClicked += Menu_MenuButtonClicked;
            
            MessageBus.Current.Listen<StatusData>().Subscribe(x => Status = x);
        }

        private void Menu_MenuButtonClicked(object sender, SubMenuItemViewModel e)
        {
            if(e.ViewModelType == null)
            {
                return;
            }
            TabStrip.Tabs.Add(new TabStripItemViewModel
            {
                Content = App.ServiceProvider.GetService(e.ViewModelType) as ViewModelTabBase
            });
        }
    }
}
