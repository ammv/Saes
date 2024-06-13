using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHConnectedHardwareService: Saes.Protos.ModelServices.JournalInstanceForCIHConnectedHardwareService.JournalInstanceForCIHConnectedHardwareServiceBase
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

            // Тут фильтрация

            //query = query
            //    .Include(x => x.SignFile)
            //    .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCIHConnectedHardwareLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCIHConnectedHardwareDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

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

            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Specified Record ID {request.RecordID} not found"));
            }

            using var transaction = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // Adding new connected hardwares
                foreach (var hardwareId in request.ConnectedHardwaresIds)
                {
                    if (record.JournalInstanceForCihconnectedHardwares.FirstOrDefault(x => x.HardwareId == hardwareId) == null)
                    {
                        record.JournalInstanceForCihconnectedHardwares.Add(new JournalInstanceForCihconnectedHardware { HardwareId = hardwareId });
                    }
                }

                foreach (var hardware in record.JournalInstanceForCihconnectedHardwares)
                {
                    if (request.ConnectedHardwaresIds.FirstOrDefault(x => x == hardware.HardwareId) == null)
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
