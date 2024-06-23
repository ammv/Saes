using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalTechnicalRecordService : Saes.Protos.ModelServices.JournalTechnicalRecordService.JournalTechnicalRecordServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalTechnicalRecordService(SaesContext ctx, IMapper mapper, ILogger<JournalTechnicalRecordService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalTechnicalRecordLookupResponse> Search(JournalTechnicalRecordLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalTechnicalRecords.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            if (request.JournalTechnicalRecordID != null)
            {
                query = query.Where(x => x.JournalTechnicalRecordId == request.JournalTechnicalRecordID);
                goto EndFilters;
            }

            query = request.OrganizationID != null ? query.Where(x => x.OrganizationId == request.OrganizationID) : query;
            query = request.KeyDocumentTypeID != null ? query.Where(x => x.KeyDocumentTypeId == request.KeyDocumentTypeID) : query;
            query = request.Note != null ? query.Where(x => x.Note.Contains(request.Note)) : query;
            query = request.ActNumber != null ? query.Where(x => x.ActNumber.Contains(request.ActNumber)) : query;
            query = request.NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted != null ? query.Where(x => x.NumberOneTimeKeyCarrierCpizoneCryptoKeysInserted.Contains(request.NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted)) : query;
            query = request.RecordOnMaintenanceCPI != null ? query.Where(x => x.RecordOnMaintenanceCpi.Contains(request.RecordOnMaintenanceCPI)) : query;
            query = request.TypeAndSerialUsedCPI != null ? query.Where(x => x.TypeAndSerialUsedCpi.Contains(request.TypeAndSerialUsedCPI)) : query;
            query = request.SerialCPIAndKeyDocumentInstanceNumber != null ? query.Where(x => x.SerialCpiandKeyDocumentInstanceNumber.Contains(request.SerialCPIAndKeyDocumentInstanceNumber)) : query;
            query = request.DestructionDate != null ? query.Where(x => x.DestructionDate.Value == request.DestructionDate.ToDateTime().ToLocalTime()) : query;

            EndFilters:

            // Тут фильтрация

            query = query
                .Include(x => x.KeyDocumentType)
                .Include(x => x.Organization)
                    .ThenInclude(o => o.BusinessEntity)
                    .ThenInclude(o => o.BusinessEntityType)
                .Include(x => x.SignFile);

            var response = new JournalTechnicalRecordLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<JournalTechnicalRecordDto>(_mapper.Config)));

            return response;
        }

        public override async Task<JournalTechnicalRecordLookupResponse> Add(JournalTechnicalRecordDataRequest request, ServerCallContext context)
        {
            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на добавление контакта организации отсутствовал идентификатор организации"));
            }

            if (!await _ctx.Organizations.AnyAsync(x => x.OrganizationId == request.OrganizationID))
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"В запросе на добавление контакта организации был указан несуществующий идентификатор организации"));
            }

            var entity = new JournalTechnicalRecord
            {
                OrganizationId = request.OrganizationID,
                Date = request.Date?.ToDateTime().ToLocalTime(),
                TypeAndSerialUsedCpi = request.TypeAndSerialUsedCPI,
                RecordOnMaintenanceCpi = request.RecordOnMaintenanceCPI,
                KeyDocumentTypeId = request.KeyDocumentTypeID,
                SerialCpiandKeyDocumentInstanceNumber = request.SerialCPIAndKeyDocumentInstanceNumber,
                NumberOneTimeKeyCarrierCpizoneCryptoKeysInserted = request.NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted,
                DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime(),
                ActNumber = request.ActNumber,

                Note = request.Note
            };

            var entry = _ctx.JournalTechnicalRecords.Add(entity);

            await _ctx.SaveChangesAsync();

            var dto = entry.Entity.Adapt(new JournalTechnicalRecordDto(), _mapper.Config);

            var response = new JournalTechnicalRecordLookupResponse();
            response.Data.Add(dto);

            return response;
        }

        public override async Task<StatusResponse> Edit(JournalTechnicalRecordDataRequest request, ServerCallContext context)
        {
            if (!request.JournalTechnicalRecordID.HasValue)
            {
                throw new ArgumentException($"{nameof(request.JournalTechnicalRecordID)} was null");
            }

            var entity = await _ctx.JournalTechnicalRecords.FirstOrDefaultAsync(x => x.JournalTechnicalRecordId == request.JournalTechnicalRecordID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.JournalTechnicalRecordID.ToString()!
                ));
            }

            entity.OrganizationId = request.OrganizationID;
            entity.Date = request.Date?.ToDateTime().ToLocalTime();
            entity.TypeAndSerialUsedCpi = request.TypeAndSerialUsedCPI;
            entity.RecordOnMaintenanceCpi = request.RecordOnMaintenanceCPI;
            entity.KeyDocumentTypeId = request.KeyDocumentTypeID;
            entity.SerialCpiandKeyDocumentInstanceNumber = request.SerialCPIAndKeyDocumentInstanceNumber;
            entity.NumberOneTimeKeyCarrierCpizoneCryptoKeysInserted = request.NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted;
            entity.DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime();
            entity.ActNumber = request.ActNumber;
            entity.Note = request.Note;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(JournalTechnicalRecordLookup request, ServerCallContext context)
        {
            if (!request.JournalTechnicalRecordID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.JournalTechnicalRecordID));
            }

            var entity = await _ctx.JournalTechnicalRecords.SingleAsync(x => x.JournalTechnicalRecordId == request.JournalTechnicalRecordID);

            _ctx.JournalTechnicalRecords.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
