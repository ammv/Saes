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
    public class KeyHolderTypeService : Saes.Protos.ModelServices.KeyHolderTypeService.KeyHolderTypeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<KeyHolderTypeService> _logger;
        private readonly IMapper _mapper;

        public KeyHolderTypeService(SaesContext saesContext, ILogger<KeyHolderTypeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.KeyHolderTypeLookupResponse> Search(Protos.ModelServices.KeyHolderTypeLookup request, ServerCallContext context)
        {
            var query = _ctx.KeyHolderTypes.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.KeyHolderTypeID != null ? query.Where(x => x.KeyHolderTypeId == request.KeyHolderTypeID) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

            var response = new KeyHolderTypeLookupResponse();

            var dtos = await query.ProjectToType<Protos.KeyHolderTypeDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<KeyHolderTypeLookupResponse> Add(KeyHolderTypeDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            var entity = new KeyHolderType
            {
                Name = request.Name
            };

            entity = (await _ctx.KeyHolderTypes.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new KeyHolderTypeLookupResponse();
            response.Data.Add(entity.Adapt<KeyHolderTypeDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(KeyHolderTypeDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            if (!request.KeyHolderTypeID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.KeyHolderTypeID));
            }

            var entity = await _ctx.KeyHolderTypes.SingleAsync(x => x.KeyHolderTypeId == request.KeyHolderTypeID);

            if (await _ctx.KeyHolderTypes.AnyAsync(x => x.Name == request.Name && x.KeyHolderTypeId != entity.KeyHolderTypeId))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.Name}"));
            }

            entity.Name = request.Name;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(KeyHolderTypeLookup request, ServerCallContext context)
        {
            if (!request.KeyHolderTypeID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.KeyHolderTypeID));
            }

            var entity = await _ctx.KeyHolderTypes.SingleAsync(x => x.KeyHolderTypeId == request.KeyHolderTypeID);

            _ctx.KeyHolderTypes.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
