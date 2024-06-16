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
    public class BusinessEntityService : Saes.Protos.ModelServices.BusinessEntityService.BusinessEntityServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<BusinessEntityService> _logger;
        private readonly IMapper _mapper;

        public BusinessEntityService(SaesContext saesContext, ILogger<BusinessEntityService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.BusinessEntityLookupResponse> Search(Protos.ModelServices.BusinessEntityLookup request, ServerCallContext context)
        {
            var query = _ctx.BusinessEntities.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            query = request.BusinessEntityTypeID != null ? query.Where(x => x.BusinessEntityTypeId == request.BusinessEntityTypeID) : query;

            var response = new BusinessEntityLookupResponse();

            var dtos = await query.ProjectToType<Protos.BusinessEntityDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<BusinessEntityLookupResponse> Add(BusinessEntityDataRequest request, ServerCallContext context)
        {
            return await base.Add(request, context);
        } 

        public override async Task<StatusResponse> Edit(BusinessEntityDataRequest request, ServerCallContext context)
        {
            return await base.Edit(request, context);
        }
    }
}
