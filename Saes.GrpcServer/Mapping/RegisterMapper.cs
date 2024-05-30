﻿using Google.Protobuf.WellKnownTypes;
using Mapster;
using Saes.Models;
using Saes.Protos;

namespace Saes.GrpcServer.Mapping
{
    public class RegisterMapper : IRegister
    {
        public Timestamp DtoT(DateTime? datetime)
        {
            if(datetime.HasValue)
            {
                return Timestamp.FromDateTime(datetime.Value.ToUniversalTime());
            }
            return null;
        }
        public void Register(TypeAdapterConfig config)
        {
            bool requireDms = false;
            bool preserveRef = true;
            config.NewConfig<Models.File, FileDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<UserRole, UserRoleDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<RightGroup, RightGroupDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<Right, RightDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<UserRoleRight, UserRoleRightDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<User, UserDto>()
                .Map(dest => dest.LastLoginDate, src => DtoT(src.LastLoginDate))
                .Map(dest => dest.UserRoleDto, src => src.UserRole)
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<Address,AddressDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<BusinessEntityType, BusinessEntityTypeDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<BusinessEntity, BusinessEntityDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.BusinessEntityTypeDto, src => src.BusinessEntityType);
            config.NewConfig<Organization, OrganizationDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.DateOfAssignmentOGRN, src => DtoT(src.DateOfAssignmentOgrn))
               .Map(dest => dest.BusinessAddressDto, src => src.BusinessAddress)
               .Map(dest => dest.BusinessEntityDto, src => src.BusinessEntity);
            config.NewConfig<ContactType, ContactTypeDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef);
            config.NewConfig<OrganizationContact, OrganizationContactDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.OrganizationDto, src => src.Organization)
               .Map(dest => dest.ContactTypeDto, src => src.ContactType);
            config.NewConfig<EmployeePosition, EmployeePositionDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<Employee, EmployeeDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.BusinessEntityDto, src => src.BusinessEntity)
               .Map(dest => dest.OrganizationDto, src => src.Organization)
               .Map(dest => dest.EmployeePositionDto, src => src.EmployeePosition);
            config.NewConfig<TableDatum, TableDataDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<TableColumnDatum, TableColumnDataDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.TableDataDto, src => src.TableData);
            config.NewConfig<LogAuthentication, LogAuthenticationDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.Date, src => DtoT(src.Date));
            config.NewConfig<UserSession, UserSessionDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.LogAuthenticationDto, src => src.LogAuthentication)
                .Map(dest => dest.CreatedAt, src => DtoT(src.CreatedAt))
                .Map(dest => dest.ExpiredAt, src => DtoT(src.ExpiredAt));
            config.NewConfig<ErrorLog, ErrorLogDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.ErrorTime, src => DtoT(src.ErrorTime))
                .Map(dest => dest.UserSessionDto, src => src.UserSession);
            config.NewConfig<Log, LogDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.Date, src => DtoT(src.Date))
                .Map(dest => dest.TableDataDto, src => src.TableData)
                .Map(dest => dest.UserSessionDto, src => src.UserSession);
            config.NewConfig<LogChange, LogChangeDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef).Map(dest => dest.TableColumnDataDto, src => src.TableColumnData);
            config.NewConfig<JournalInstanceForCparecord, JournalInstanceForCPARecordDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.DestructionDate, src => DtoT(src.DestructionDate))
                .Map(dest => dest.CommissioningDate, src => DtoT(src.CommissioningDate))
                .Map(dest => dest.DecommissioningDate, src => DtoT(src.DecommissioningDate))
                .Map(dest => dest.SignFileDto, src => src.SignFile)
                .Map(dest => dest.BusinessEntityDto, src => src.ReceivedFrom)
                .Map(dest => dest.OrganizationDto, src => src.Organization);
            config.NewConfig<JournalInstanceCpareceiver, JournalInstanceCPAReceiverDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.RecordDto, src => src.Record)
                .Map(dest => dest.ReceiverDto, src => src.Receiver);
            config.NewConfig<Hardware, HardwareDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.OrganizationDto, src => src.Organization);
            config.NewConfig<JournalInstanceForCihrecord, JournalInstanceForCIHRecordDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.DestructionDate, src => DtoT(src.DestructionDate))
               .Map(dest => dest.ReceivedFromDto, src => src.ReceivedFrom)
               .Map(dest => dest.CPIUserDto, src => src.Cpiuser)
               .Map(dest => dest.SignFileDto, src => src.SignFile);
            config.NewConfig<JournalInstanceForCihinstaller, JournalInstanceForCIHInstallerDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.RecordDto, src => src.Record)
                .Map(dest => dest.InstallerDto, src => src.Installer);
            config.NewConfig<JournalInstanceForCihconnectedHardware, JournalInstanceForCIHConnectedHardwareDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.RecordDto, src => src.Record)
                .Map(dest => dest.HardwareDto, src => src.Hardware);
            config.NewConfig<JournalInstanceForCihdestructor, JournalInstanceForCIHDestructorDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.RecordDto, src => src.Record)
                .Map(dest => dest.DestructorDto, src => src.Destructor);
            config.NewConfig<KeyDocumentType, KeyDocumentTypeDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<KeyHolderType, KeyHolderTypeDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<KeyHolder, KeyHolderDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef)
                .Map(dest => dest.KeyHolderTypeDto, src => src.Type)
                .Map(dest => dest.UserCPIDto, src => src.UserCpi)
                .Map(dest => dest.SignFileDto, src => src.SignFile);
            config.NewConfig<JournalTechnicalRecord, JournalTechnicalRecordDto>()
               .RequireDestinationMemberSource(requireDms)
               .PreserveReference(preserveRef)
               .Map(dest => dest.Date, src => DtoT(src.Date))
               .Map(dest => dest.DestructionDate, src => DtoT(src.DestructionDate))
               .Map(dest => dest.OrganizationDto, src => src.Organization)
               .Map(dest => dest.KeyDocumentTypeDto, src => src.KeyDocumentType)
               .Map(dest => dest.SignFileDto, src => src.SignFile);
        }
    }
}
