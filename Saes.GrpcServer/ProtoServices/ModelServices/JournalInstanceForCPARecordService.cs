using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCPARecordService: Saes.Protos.ModelServices.JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCPARecordService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCPARecordService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCPARecordLookupResponse> Search(JournalInstanceForCPARecordLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCparecords.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.OrganizationID != null ? query.Where(x => x.OrganizationId == request.OrganizationID) : query;

            query = request.JournalInstanceForCPARecordID != null ? query.Where(x => x.JournalInstanceForCparecordId == request.JournalInstanceForCPARecordID) : query;

            query = request.NameCPI != null ? query.Where(x => x.NameCpi.Contains(request.NameCPI)) : query;

            query = request.SerialCPI != null ? query.Where(x => x.SerialCpi.Contains(request.SerialCPI)) : query;

            query = request.InstanceNumber != null ? query.Where(x => x.InstanceNumber == request.InstanceNumber) : query;

            query = request.ReceivedFromID != null ? query.Where(x => x.ReceivedFromId == request.ReceivedFromID) : query;


            query = request.DateAndNumberCoverLetterReceive != null ? query.Where(x => x.DateAndNumberCoverLetterReceive.Contains(request.DateAndNumberCoverLetterReceive)) : query;

            query = request.DateAndNumberCoverLetterSend != null ? query.Where(x => x.DateAndNumberCoverLetterSend.Contains(request.DateAndNumberCoverLetterSend)) : query;

            query = request.DateAndNumberConfirmationSend != null ? query.Where(x => x.DateAndNumberConfirmationSend.Contains(request.DateAndNumberConfirmationSend)) : query;

            query = request.DateAndNumberCoverLetterReturn != null ? query.Where(x => x.DateAndNumberCoverLetterReturn.Contains(request.DateAndNumberCoverLetterReturn)) : query;

            query = request.DateAndNumberConfirmationReturn != null ? query.Where(x => x.DateAndNumberConfirmationReturn.Contains(request.DateAndNumberConfirmationReturn)) : query;

            query = request.DestructionActNumber != null ? query.Where(x => x.DestructionActNumber.Contains(request.DestructionActNumber)) : query;

            var response = new JournalInstanceForCPARecordLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCPARecordDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<JournalInstanceForCPARecordLookupResponse> Add(JournalInstanceForCPARecordDataRequest request, ServerCallContext context)
        {
            var entity = new JournalInstanceForCparecord
            {
                OrganizationId = request.OrganizationID,
                NameCpi = request.NameCPI,
                SerialCpi = request.SerialCPI,
                InstanceNumber = request.InstanceNumber,
                ReceivedFromId = request.ReceivedFromID,

                DateAndNumberCoverLetterReceive = request.DateAndNumberCoverLetterReceive,
                DateAndNumberCoverLetterSend = request.DateAndNumberCoverLetterSend,

                DateAndNumberConfirmationSend = request.DateAndNumberConfirmationSend,
                DateAndNumberCoverLetterReturn = request.DateAndNumberCoverLetterReturn,

                

                DateAndNumberConfirmationReturn = request.DateAndNumberConfirmationReturn,

                CommissioningDate = request.CommissioningDate?.ToDateTime().ToLocalTime(),
                DecommissioningDate = request.DecommissioningDate?.ToDateTime().ToLocalTime(),
                DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime(),
                DestructionActNumber = request.DestructionActNumber,
                Note = request.Note
            };

            entity = (await _ctx.JournalInstanceForCparecords.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var dto = entity.Adapt(new JournalInstanceForCPARecordDto(), _mapper.Config);

            var response = new JournalInstanceForCPARecordLookupResponse();
            response.Data.Add(dto);

            return response;
        }

        public override async Task<StatusResponse> Edit(JournalInstanceForCPARecordDataRequest request, ServerCallContext context)
        {
            if(!request.JournalInstanceForCPARecordID.HasValue)
            {
                throw new ArgumentException($"{nameof(request.JournalInstanceForCPARecordID)} was null");
            }

            if (await _ctx.JournalInstanceForCparecords.SingleAsync(
                x => x.JournalInstanceForCparecordId == request.JournalInstanceForCPARecordID) == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.JournalInstanceForCPARecordID.ToString()!
                ));
            }

            var entity = await _ctx.JournalInstanceForCparecords.SingleAsync(x => x.JournalInstanceForCparecordId == request.JournalInstanceForCPARecordID);


            //ntity.JournalInstanceForCparecordId = request.JournalInstanceForCPARecordID.Value;
            entity.OrganizationId = request.OrganizationID;
            entity.NameCpi = request.NameCPI;
            entity.SerialCpi = request.SerialCPI;
            entity.InstanceNumber = request.InstanceNumber;
            entity.ReceivedFromId = request.ReceivedFromID;
            entity.DateAndNumberCoverLetterReceive = request.DateAndNumberCoverLetterReceive;
            entity.DateAndNumberCoverLetterSend = request.DateAndNumberCoverLetterSend;
            entity.DateAndNumberConfirmationSend = request.DateAndNumberConfirmationSend;
            entity.DateAndNumberCoverLetterReturn = request.DateAndNumberCoverLetterReturn;
            entity.DateAndNumberConfirmationReturn = request.DateAndNumberConfirmationReturn;
            entity.CommissioningDate = request.CommissioningDate?.ToDateTime().ToLocalTime();
            entity.DecommissioningDate = request.DecommissioningDate?.ToDateTime().ToLocalTime();
            entity.DestructionDate = request.DestructionDate?.ToDateTime().ToLocalTime();
            entity.DestructionActNumber = request.DestructionActNumber;
            entity.Note = request.Note;

            _ctx.JournalInstanceForCparecords.Update(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };

        }

        public override async Task<StatusResponse> Remove(JournalInstanceForCPARecordLookup request, ServerCallContext context)
        {
            if (!request.JournalInstanceForCPARecordID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.JournalInstanceForCPARecordID));
            }

            var entity = await _ctx.JournalInstanceForCparecords.FirstOrDefaultAsync(x => x.JournalInstanceForCparecordId == request.JournalInstanceForCPARecordID);

            if(entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.JournalInstanceForCPARecordID.ToString()!
                ));
            }

            _ctx.JournalInstanceForCparecords.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
