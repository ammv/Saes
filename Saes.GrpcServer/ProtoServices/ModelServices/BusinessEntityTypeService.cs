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
    public class BusinessEntityTypeService : Saes.Protos.ModelServices.BusinessEntityTypeService.BusinessEntityTypeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<BusinessEntityTypeService> _logger;
        private readonly IMapper _mapper;

        public BusinessEntityTypeService(SaesContext saesContext, ILogger<BusinessEntityTypeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.BusinessEntityTypeLookupResponse> Search(Protos.ModelServices.BusinessEntityTypeLookup request, ServerCallContext context)
        {
            var query = _ctx.BusinessEntityTypes.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Filters

            var response = new BusinessEntityTypeLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<BusinessEntityTypeDto>(_mapper.Config)));

            return response;
        }

        public override async Task<BusinessEntityTypeLookupResponse> Add(BusinessEntityTypeDataRequest request, ServerCallContext context)
        {
            return await base.Add(request, context);
        } 

        public override async Task<StatusResponse> Edit(BusinessEntityTypeDataRequest request, ServerCallContext context)
        {
            return await base.Edit(request, context);
        }
    }
}
