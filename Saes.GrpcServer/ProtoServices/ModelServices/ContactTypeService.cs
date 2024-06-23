using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Core;
using Saes.Protos;
using Saes.Protos.ModelServices;

#nullable disable

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class ContactTypeService : Saes.Protos.ModelServices.ContactTypeService.ContactTypeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<ContactTypeService> _logger;
        private readonly IMapper _mapper;

        public ContactTypeService(SaesContext saesContext, ILogger<ContactTypeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.ContactTypeLookupResponse> Search(Protos.ModelServices.ContactTypeLookup request, ServerCallContext context)
        {
            var query = _ctx.ContactTypes.AsQueryable();

            if(request.ContactTypeId != null)
            {
                query = query.Where(x => x.ContactTypeId == request.ContactTypeId);
                goto EndFilters;
            }

            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

            EndFilters:

            var response = new ContactTypeLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<ContactTypeDto>(_mapper.Config)));

            return response;
        }

        public override Task<ContactTypeLookupResponse> Add(ContactTypeDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(ContactTypeDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(ContactTypeLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
