using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceCPAReceiverService : Saes.Protos.ModelServices.JournalInstanceCPAReceiverService.JournalInstanceCPAReceiverServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceCPAReceiverService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceCPAReceiverService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<JournalInstanceCPAReceiverLookupResponse> Search(JournalInstanceCPAReceiverLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceCpareceivers.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.ReceiverID != null ? query.Where(x => x.ReceiverId == request.ReceiverID) : query;
            query = request.JournalInstanceCPAReceiverID != null ? query.Where(x => x.JournalInstanceCpareceiverId == request.JournalInstanceCPAReceiverID) : query;
            query = request.RecordID != null ? query.Where(x => x.RecordId == request.RecordID) : query;

            //query = query
            //    .Include(x => x.SignFile)
            //    .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceCPAReceiverLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceCPAReceiverDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<JournalInstanceCPAReceiverLookupResponse> Add(JournalInstanceCPAReceiverDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }


        public override Task<StatusResponse> Edit(JournalInstanceCPAReceiverDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceCPAReceiverLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }

        public override async Task<StatusResponse> BulkUpdate(JournalInstanceCPAReceiverBulkUpdateRequest request, ServerCallContext context)
        {
            var record = await _ctx.JournalInstanceForCparecords.Include(x => x.JournalInstanceCpareceivers).FirstOrDefaultAsync(x => x.JournalInstanceForCparecordId == request.RecordID);

            if (record == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Specified Record ID {request.RecordID} not found"));
            }

            using var transaction = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // Adding new installers
                foreach (var receiverId in request.ReceiversIds)
                {
                    var installer = record.JournalInstanceCpareceivers.FirstOrDefault(x => x.ReceiverId == receiverId);
                    if (installer != null)
                    {
                        if (installer.SysIsDeleted)
                        {
                            installer.SysIsDeleted = false;

                        }
                    }
                    else
                    {
                        record.JournalInstanceCpareceivers.Add(new JournalInstanceCpareceiver { ReceiverId = receiverId });
                    }
                }

                foreach (var receiver in record.JournalInstanceCpareceivers)
                {
                    if (request.ReceiversIds.FirstOrDefault(x => x == receiver.ReceiverId) == 0)
                    {
                        record.JournalInstanceCpareceivers.Remove(receiver);
                    }
                }

                await _ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                return new StatusResponse { Result = true };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new StatusResponse { Result = false };
            }

        }
    }
}
