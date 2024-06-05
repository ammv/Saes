using Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.LogAuthentication;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.ErrorLog;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication.User;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceCPAReceiver;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHConnectedHardware;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHDestructor;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHInstaller;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHRecord;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalTechnicalRecord;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyDocumentType;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyHolder;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyHolderType;
using Saes.AvaloniaMvvmClient.ViewModels.Other;
using Saes.AvaloniaMvvmClient.Views.ElectricitySigns.KeyDocumentType;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.BusinessEntity;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.BusinessEntityType;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Organization;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Employee;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.EmployeePosition;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.OrganizationContact;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserRole;
using Saes.AvaloniaMvvmClient.ViewModels.Office.Hardware;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.Right;
using Saes.AvaloniaMvvmClient.ViewModels.Person.ContactType;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserSession;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.TableData;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.TableDataColumn;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.RightGroup;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.LogChange;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserRoleRight;
using Saes.AvaloniaMvvmClient.ViewModels.Other.Address;
using Saes.AvaloniaMvvmClient.ViewModels.Other.File;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.Log;
using Saes.AvaloniaMvvmClient.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Saes.AvaloniaMvvmClient.ViewModels.MainMenu
{
    public class MainMenuViewModel: ViewModelBase
    {

        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        public TabStripViewModel TabStrip { get; }
        public SideMenuViewModel Menu { get; }

        [Reactive]
        public StatusData Status { get; set; }
        public INavigationService NavigationService { get; }

        private async Task OnExitCommand()
        {
            var result = await MessageBoxHelper.Question("Вопрос", "Вы уверены, что хотите выйти из аккаунта?");
            if(result)
            {
                NavigationService.NavigateTo(App.ServiceProvider.GetService<AuthenticationMainViewModel>());
            }

        }

        public MainMenuViewModel(INavigationService navigationService)
        {
            ExitCommand = ReactiveCommand.CreateFromTask(OnExitCommand);

            TabStrip = new TabStripViewModel(new ObservableCollection<TabStripItemViewModel>());

            Menu = new SideMenuViewModel(
                new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel("Аудит", "send_logging_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Данные таблиц", "table_freeze_regular", typeof(TableDataListViewModel)),
                        new SubMenuItemViewModel("Данные столбцов таблиц", "column_triple_regular", typeof(TableDataColumnListViewModel)),
                        new SubMenuItemViewModel("Логи аутенфикаций", "person_arrow_right_regular", typeof(LogAuthenticationListViewModel)),
                        new SubMenuItemViewModel("Логи ошибок", "error_circle_regular", typeof(ErrorLogListViewModel)),
                        new SubMenuItemViewModel("Логи действий", "book_database_regular", typeof(LogListViewModel)),
                        new SubMenuItemViewModel("Логи изменений", "book_database_regular", typeof(LogChangeListViewModel)),
                    }),
                    new MenuItemViewModel("Авторизация", "people_community_add_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                        new SubMenuItemViewModel("Роли пользователей", "book_regular", typeof(UserRoleListViewModel)),
                        new SubMenuItemViewModel("Группы прав", "book_regular", typeof(RightGroupListViewModel)),
                        new SubMenuItemViewModel("Права", "book_regular", typeof(RightListViewModel)),
                        new SubMenuItemViewModel("Права ролей", "book_regular", typeof(UserRoleRightListViewModel)),
                        new SubMenuItemViewModel("Сессии", "share_screen_regular", typeof(UserSessionListViewModel)),
                        
                    }),
                    new MenuItemViewModel("Аутенфикация", "fingerprint_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Пользователи", "people_team_regular", typeof(UserListViewModel)),
                    }),
                    new MenuItemViewModel("Человеческие ресурсы", "people_team_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Типы бизнес-сущностей", "book_regular", typeof(BusinessEntityTypeListViewModel)),
                         new SubMenuItemViewModel("Бизнес-сущности", "people_team_regular", typeof(BusinessEntityListViewModel)),
                         new SubMenuItemViewModel("Организации", "briefcase_regular", typeof(OrganizationListViewModel)),
                         new SubMenuItemViewModel("Контакты организаций", "call_regular", typeof(OrganizationContactListViewModel)),
                         new SubMenuItemViewModel("Должности сотрудников", "book_regular", typeof(EmployeePositionListViewModel)),
                         new SubMenuItemViewModel("Сотрудники", "people_team_regular", typeof(EmployeeListViewModel)),
                    }),
                    new MenuItemViewModel("Личная информация", "contact_card_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Типы контактов", "book_regular", typeof(ContactTypeListViewModel)),
                         
                    }),
                    new MenuItemViewModel("Электронные подписи", "signature_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Журнал технический (аппаратный) ", "notebook_regular", typeof(JournalTechnicalRecordListViewModel)),
                         new SubMenuItemViewModel("Журнал поэкземплярного учета СКЗИ для органа криптографической защиты", "notebook_regular", typeof(JournalInstanceForCPARecordListViewModel)),
                         new SubMenuItemViewModel("Журнал поэкземплярного учета СКЗИ, эксплуатационной и технической документации к ним, ключевых документов (для обладателя конфиденциальной информации) ", "notebook_regular", typeof(JournalInstanceForCIHRecordListViewModel)),
                         new SubMenuItemViewModel("Субъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которым была разослана информация", "people_community_regular", typeof(JournalInstanceCPAReceiverListViewModel)),
                         new SubMenuItemViewModel("Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, производивших подключение (установку)",
                         "people_community_regular", typeof(JournalInstanceForCIHInstallerListViewModel)
                         ),
                         new SubMenuItemViewModel("Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, производивших изъятие (уничтожение)", "people_community_regular", typeof(JournalInstanceForCIHDestructorListViewModel)),
                         new SubMenuItemViewModel("Номера аппаратных средств, в которые установлены или к которым подключены СКЗИ", "notebook_regular", typeof(JournalInstanceForCIHConnectedHardwareListViewModel)),
                         new SubMenuItemViewModel("Типы ключевых документов", "document_page_top_right_regular", typeof(KeyDocumentTypeListViewModel)),
                         new SubMenuItemViewModel("Типы ключевых носителей", "usb_stick_regular", typeof(KeyHolderTypeListViewModel)),
                         new SubMenuItemViewModel("Ключевые носители", "key_regular", typeof(KeyHolderListViewModel)),

                    }),
                    new MenuItemViewModel("Офис", "office_chair_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Аппаратура", "phone_laptop_regular", typeof(HardwareListViewModel)),
                    }),
                    new MenuItemViewModel("Прочее", "book_question_mark_regular", new ObservableCollection<SubMenuItemViewModel>()
                    {
                         new SubMenuItemViewModel("Файлы", "document_regular", typeof(FileListViewModel)),
                         new SubMenuItemViewModel("Адреса", "location_regular", typeof(AddressListViewModel)),
                    }),
                }
            );

            Menu.SideMenuItemClicked += Menu_MenuButtonClicked;
            
            MessageBus.Current.Listen<StatusData>().Subscribe(x => Status = x);
            NavigationService = navigationService;
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
