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
    public class AddressService: Protos.ModelServices.AddressService.AddressServiceBase
    {
        private readonly ILogger<AddressService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public AddressService(ILogger<AddressService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<AddressLookupResponse> Search(AddressLookup request, ServerCallContext context)
        {

            var query = _ctx.Addresses.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            //query = request.AddressId != null ? query.Where(x => x.AddressId == request.AddressId) : query;
            //query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

           // query = query.Include(x => x.Address);

            var response = new AddressLookupResponse();

            var dtos = await query.ProjectToType<AddressDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<AddressLookupResponse> Add(AddressDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(AddressDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(AddressLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
