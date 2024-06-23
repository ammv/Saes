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
    public class RightGroupService : Protos.ModelServices.RightGroupService.RightGroupServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<RightGroupService> _logger;
        private readonly IMapper _mapper;

        public RightGroupService(SaesContext saesContext, ILogger<RightGroupService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<RightGroupLookupResponse> Search(RightGroupLookup request, ServerCallContext context)
        {
            var query = _ctx.RightGroups.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            if(request.RightGroupId != null)
            {
                query = query.Where(x => x.RightGroupId == request.RightGroupId);
                goto EndFilters;
            }

            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;
            query = request.Code != null ? query.Where(x => x.Code.Contains(request.Code)) : query;

            EndFilters:

            var response = new RightGroupLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<RightGroupDto>(_mapper.Config)));

            return response;
        }
    }
}
