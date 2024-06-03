using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class HardwareService: Protos.ModelServices.HardwareService.HardwareServiceBase
    {
        private readonly ILogger<HardwareService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public HardwareService(ILogger<HardwareService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<HardwareLookupResponse> Search(HardwareLookup request, ServerCallContext context)
        {

            var query = _ctx.Hardwares.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.HardwareId != null ? query.Where(x => x.HardwareId == request.HardwareId) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

           // query = query.Include(x => x.Hardware);

            var response = new HardwareLookupResponse();

            var dtos = await query.ProjectToType<HardwareDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<HardwareLookupResponse> Add(HardwareDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(HardwareDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(HardwareLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
