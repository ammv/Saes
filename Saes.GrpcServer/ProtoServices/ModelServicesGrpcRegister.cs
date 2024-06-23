using Saes.GrpcServer.ProtoServices.ModelServices;

namespace Saes.GrpcServer.ProtoServices
{
    public class ModelServicesGrpcRegister
    {
        public void Register(WebApplication? app)
        {
            app.MapGrpcService<UserService>();
            app.MapGrpcService<UserRoleService>();
            app.MapGrpcService<UserRoleRightService>();
            app.MapGrpcService<JournalInstanceForCIHConnectedHardwareService>();
            app.MapGrpcService<JournalInstanceForCIHDestructorService>();
            app.MapGrpcService<JournalInstanceForCIHInstallerService>();
            app.MapGrpcService<JournalInstanceForCIHRecordService>();
            app.MapGrpcService<JournalInstanceForCPARecordService>();
            app.MapGrpcService<JournalTechnicalRecordService>();
            app.MapGrpcService<KeyDocumentTypeService>();
            app.MapGrpcService<KeyHolderService>();
            app.MapGrpcService<KeyHolderTypeService>();
            app.MapGrpcService<LogAuthenticationService>();
            app.MapGrpcService<OrganizationService>();
            app.MapGrpcService<ErrorLogService>();
            app.MapGrpcService<EmployeeService>();
            app.MapGrpcService<EmployeePositionService>();
            app.MapGrpcService<OrganizationContactService>();
            app.MapGrpcService<BusinessEntityService>();
            app.MapGrpcService<BusinessEntityTypeService>();
            app.MapGrpcService<HardwareService>();
            app.MapGrpcService<RightService>();
            app.MapGrpcService<RightGroupService>();
            app.MapGrpcService<UserSessionService>();
            app.MapGrpcService<TableColumnDataService>();
            app.MapGrpcService<TableDataService>();
            app.MapGrpcService<LogService>();
            app.MapGrpcService<LogChangeService>();
            app.MapGrpcService<AddressService>();
            app.MapGrpcService<FileService>();
            app.MapGrpcService<ContactTypeService>();
        }
    }
}
