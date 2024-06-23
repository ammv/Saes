using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Core;
using Saes.Protos;
using Saes.Protos.ModelServices;

#nullable disable

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class RightService : Saes.Protos.ModelServices.RightService.RightServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<RightService> _logger;
        private readonly IMapper _mapper;

        public RightService(SaesContext saesContext, ILogger<RightService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.RightLookupResponse> Search(Protos.ModelServices.RightLookup request, ServerCallContext context)
        {
            var query = _ctx.Rights.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            if(request.RightId != null)
            {
                query = query.Where(x => x.RightId == request.RightId);
                goto EndFilters;
            }

            query = request.Code != null ? query.Where(x => x.Code.Contains(request.Code)) : query;
            query = request.RightGroupId != null ? query.Where(x => x.RightGroupId == request.RightGroupId) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

            EndFilters:

            query = query.Include(x => x.RightGroup);

            var response = new RightLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<RightDto>(_mapper.Config)));

            return response;
        }
    }
}
