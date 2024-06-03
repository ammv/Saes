
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Interceptors;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.ErrorLog;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.Log;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.LogAuthentication;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.TableData;
using Saes.AvaloniaMvvmClient.ViewModels.Audit.TableDataColumn;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication.User;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.Right;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.RightGroup;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserRole;
using Saes.AvaloniaMvvmClient.ViewModels.Authorization.UserSession;
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
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.BusinessEntity;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.BusinessEntityType;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Employee;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.EmployeePosition;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Organization;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.OrganizationContact;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.AvaloniaMvvmClient.ViewModels.Office.Hardware;
using Saes.AvaloniaMvvmClient.ViewModels.Person.ContactType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Injections
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton<ISessionKeyService, FileSessionKeyService>();
            collection.AddSingleton<IUserService, UserService>();
            collection.AddSingleton<IDialogService, DialogService>();
        }

        public static void AddGrpcServices(this IServiceCollection collection)
        {
            collection.AddTransient<SessionKeyInterceptor>();
            collection.AddTransient<StatusLoggingInterceptor>();
            collection.AddSingleton<IGrpcChannelFactory>(sp =>
            {
                return new GrpcChannelFactory("https://localhost:7231", sp);
            });

        }

        public static void AddMainViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<MainViewModel>();
            collection.AddTransient<MainMenuViewModel>();
                        collection.AddTransient<JournalInstanceForCPARecordListViewModel>();

        }

        public static void AddAuthenticationViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<AuthenticationMainViewModel>();
            collection.AddTransient<FirstFactorAuthenticationViewModel>();
            collection.AddTransient<SecondFactorAuthenticationViewModel>();
            collection.AddTransient<UserListViewModel>();
            collection.AddTransient<UserFormViewModel>();
        }

        public static void AddElectricitySignsViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<JournalInstanceCPAReceiverListViewModel>();
            collection.AddTransient<JournalInstanceForCIHConnectedHardwareListViewModel>();
            collection.AddTransient<JournalInstanceForCIHDestructorListViewModel>();
            collection.AddTransient<JournalInstanceForCIHInstallerListViewModel>();
            collection.AddTransient<JournalInstanceForCIHRecordListViewModel>();
            collection.AddTransient<JournalInstanceForCPARecordListViewModel>();
            collection.AddTransient<JournalTechnicalRecordListViewModel>();
            collection.AddTransient<KeyDocumentTypeListViewModel>();
            collection.AddTransient<KeyHolderTypeListViewModel>();
            collection.AddTransient<KeyHolderListViewModel>();
        }

        public static void AddAuditViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<ErrorLogListViewModel>();
            collection.AddTransient<LogAuthenticationListViewModel>();
            collection.AddTransient<LogListViewModel>();
            collection.AddTransient<TableDataListViewModel>();
            collection.AddTransient<TableDataColumnListViewModel>();
        }

        public static void AddAuthorizationViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<UserRoleListViewModel>();
            collection.AddTransient<RightListViewModel>();
            collection.AddTransient<UserSessionListViewModel>();
            collection.AddTransient<RightGroupListViewModel>();
        }

        public static void AddOfficeViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<HardwareListViewModel>();
        }

        public static void AddPersonViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<ContactTypeListViewModel>();
        }

        public static void AddHumanResourcesViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<BusinessEntityTypeListViewModel>();
            collection.AddTransient<BusinessEntityListViewModel>();
            collection.AddTransient<OrganizationListViewModel>();
            collection.AddTransient<EmployeeListViewModel>();
            collection.AddTransient<EmployeePositionListViewModel>();
            collection.AddTransient<OrganizationContactListViewModel>();
        }
    }
}
