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
    public class RightGroupService : Saes.Protos.ModelServices.RightGroupService.RightGroupServiceBase
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

        public override async Task<Protos.ModelServices.RightGroupLookupResponse> Search(Protos.ModelServices.RightGroupLookup request, ServerCallContext context)
        {
            var query = _ctx.RightGroups.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.RightGroupID != null ? query.Where(x => x.RightGroupId == request.RightGroupID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new RightGroupLookupResponse();

            var dtos = await query.ProjectToType<Protos.RightGroupDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<RightGroupLookupResponse> Add(RightGroupDataRequest request, ServerCallContext context)
        {
            return await base.Add(request, context);
        }

        public override async Task<StatusResponse> Edit(RightGroupDataRequest request, ServerCallContext context)
        {
            return await base.Edit(request, context);
        }
    }
}
