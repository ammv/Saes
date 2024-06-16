using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHRecordService: Saes.Protos.ModelServices.JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCIHRecordService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCIHRecordService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCIHRecordLookupResponse> Search(JournalInstanceForCIHRecordLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCihrecords.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Тут фильтрация

            query = query
                .Include(x => x.Organization)
                .Include(x => x.SignFile)
                .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCIHRecordLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCIHRecordDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<JournalInstanceForCIHRecordLookupResponse> Add(JournalInstanceForCIHRecordDataRequest request, ServerCallContext context)
        {
            var entity = new JournalInstanceForCihrecord
            {
                OrganizationId = request.OrganizationID,
                NameCpi = request.NameCPI,
                SerialCpi = request.SerialCPI,
                InstanceNumber = request.InstanceNumber,
                ReceivedFromId = request.ReceivedFromID,
                DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime(),
                DestructionActNumber = request.DestructionActNumber,
                CpiuserId = request.CPIUserID,
                DateAndNumberConfirmationIssue = request.DateAndNumberConfirmationIssue,
                DateAndNumberCoverLetterReceive = request.DateAndNumberCoverLetterReceive,
                InstallationDateAndConfirmation = request.InstallationDateAndConfirmation,
                Note = request.Note
            }; 

            var entry = _ctx.JournalInstanceForCihrecords.Add(entity);

            await _ctx.SaveChangesAsync();

            var dto = entry.Entity.Adapt(new JournalInstanceForCIHRecordDto(), _mapper.Config);

            var response = new JournalInstanceForCIHRecordLookupResponse();
            response.Data.Add(dto);

            return response;
        }

        public override async Task<StatusResponse> Edit(JournalInstanceForCIHRecordDataRequest request, ServerCallContext context)
        {
            if (!request.JournalInstanceForCIHRecordId.HasValue)
            {
                throw new ArgumentException($"{nameof(request.JournalInstanceForCIHRecordId)} was null");
            }

            var entity = await _ctx.JournalInstanceForCihrecords.FirstOrDefaultAsync(x => x.JournalInstanceForCihrecordId == request.JournalInstanceForCIHRecordId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.JournalInstanceForCIHRecordId.ToString()!
                ));
            }

            //ntity.JournalInstanceForCIHRecordId = request.JournalInstanceForCIHRecordID.Value;
            entity.OrganizationId = request.OrganizationID;
            entity.NameCpi = request.NameCPI;
            entity.SerialCpi = request.SerialCPI;
            entity.InstanceNumber = request.InstanceNumber;
            entity.ReceivedFromId = request.ReceivedFromID;
            entity.DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime();
            entity.DestructionActNumber = request.DestructionActNumber;
            entity.CpiuserId = request.CPIUserID;
            entity.DateAndNumberConfirmationIssue = request.DateAndNumberConfirmationIssue;
            entity.DateAndNumberCoverLetterReceive = request.DateAndNumberCoverLetterReceive;
            entity.InstallationDateAndConfirmation = request.InstallationDateAndConfirmation;
            entity.Note = request.Note;

            //_ctx.JournalInstanceForCihrecords.Update(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(JournalInstanceForCIHRecordLookup request, ServerCallContext context)
        {
            if (!request.JournalInstanceForCIHRecordId.HasValue)
            {
                throw new ArgumentNullException(nameof(request.JournalInstanceForCIHRecordId));
            }

            var entity = await _ctx.JournalInstanceForCihrecords.SingleAsync(x => x.JournalInstanceForCihrecordId == request.JournalInstanceForCIHRecordId);

            _ctx.JournalInstanceForCihrecords.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
