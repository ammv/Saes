using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHConnectedHardwareService : Saes.Protos.ModelServices.JournalInstanceForCIHConnectedHardwareService.JournalInstanceForCIHConnectedHardwareServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCIHConnectedHardwareService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCIHConnectedHardwareService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<JournalInstanceForCIHConnectedHardwareLookupResponse> Search(JournalInstanceForCIHConnectedHardwareLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCihconnectedHardwares.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.HardwareID != null ? query.Where(x => x.HardwareId == request.HardwareID) : query;
            query = request.JournalInstanceForCIHConnectedHardwareID != null ? query.Where(x => x.JournalInstanceForCihconnectedHardwareId == request.JournalInstanceForCIHConnectedHardwareID) : query;
            query = request.RecordID != null ? query.Where(x => x.RecordId == request.RecordID) : query;

            query = query
                .Include(x => x.Hardware);

            var response = new JournalInstanceForCIHConnectedHardwareLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<JournalInstanceForCIHConnectedHardwareDto>(_mapper.Config)));

            return response;
        }

        public override Task<JournalInstanceForCIHConnectedHardwareLookupResponse> Add(JournalInstanceForCIHConnectedHardwareDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(JournalInstanceForCIHConnectedHardwareDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceForCIHConnectedHardwareLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }

        public override async Task<StatusResponse> BulkUpdate(JournalInstanceForCIHConnectedHardwareBulkUpdateRequest request, ServerCallContext context)
        {
            var record = await _ctx.JournalInstanceForCihrecords.Include(x => x.JournalInstanceForCihconnectedHardwares).FirstOrDefaultAsync(x => x.JournalInstanceForCihrecordId == request.RecordID);

            if (record == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Specified Record ID {request.RecordID} not found"));
            }

            using var transaction = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // Adding new connected hardwares
                foreach (var hardwareId in request.ConnectedHardwaresIds)
                {
                    var connectedHardware = record.JournalInstanceForCihconnectedHardwares.FirstOrDefault(x => x.HardwareId == hardwareId);
                    if (connectedHardware != null)
                    {
                        if(connectedHardware.SysIsDeleted)
                        {
                            connectedHardware.SysIsDeleted = false;
                        }
                    }
                    else
                    {
                        record.JournalInstanceForCihconnectedHardwares.Add(new JournalInstanceForCihconnectedHardware { HardwareId = hardwareId });
                    }
                }

                foreach (var hardware in record.JournalInstanceForCihconnectedHardwares)
                {
                    if (request.ConnectedHardwaresIds.FirstOrDefault(x => x == hardware.HardwareId) == 0)
                    {
                        record.JournalInstanceForCihconnectedHardwares.Remove(hardware);
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