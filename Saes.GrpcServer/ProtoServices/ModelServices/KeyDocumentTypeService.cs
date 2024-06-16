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
    public class KeyDocumentTypeService : Saes.Protos.ModelServices.KeyDocumentTypeService.KeyDocumentTypeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<KeyDocumentTypeService> _logger;
        private readonly IMapper _mapper;

        public KeyDocumentTypeService(SaesContext saesContext, ILogger<KeyDocumentTypeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.KeyDocumentTypeLookupResponse> Search(Protos.ModelServices.KeyDocumentTypeLookup request, ServerCallContext context)
        {
            var query = _ctx.KeyDocumentTypes.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.KeyDocumentTypeID != null ? query.Where(x => x.KeyDocumentTypeId == request.KeyDocumentTypeID) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

            var response = new KeyDocumentTypeLookupResponse();

            var dtos = await query.ProjectToType<Protos.KeyDocumentTypeDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<KeyDocumentTypeLookupResponse> Add(KeyDocumentTypeDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            var entity = new KeyDocumentType
            {
                Name = request.Name
            };

            entity = (await _ctx.KeyDocumentTypes.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new KeyDocumentTypeLookupResponse();
            response.Data.Add(entity.Adapt<KeyDocumentTypeDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(KeyDocumentTypeDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            if (!request.KeyDocumentTypeID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.KeyDocumentTypeID));
            }

            var entity = await _ctx.KeyDocumentTypes.SingleAsync(x => x.KeyDocumentTypeId == request.KeyDocumentTypeID);

            if (await _ctx.KeyDocumentTypes.AnyAsync(x => x.Name == request.Name && x.KeyDocumentTypeId != entity.KeyDocumentTypeId))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.Name}"));
            }

            entity.Name = request.Name;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(KeyDocumentTypeLookup request, ServerCallContext context)
        {
            if (!request.KeyDocumentTypeID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.KeyDocumentTypeID));
            }

            var entity = await _ctx.KeyDocumentTypes.SingleAsync(x => x.KeyDocumentTypeId == request.KeyDocumentTypeID);

            _ctx.KeyDocumentTypes.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
