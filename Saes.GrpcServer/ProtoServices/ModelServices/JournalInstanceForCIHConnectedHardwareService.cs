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
    }
}
