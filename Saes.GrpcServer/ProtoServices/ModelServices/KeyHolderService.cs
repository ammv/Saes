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
    public class KeyHolderService : Saes.Protos.ModelServices.KeyHolderService.KeyHolderServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<KeyHolderService> _logger;
        private readonly IMapper _mapper;

        public KeyHolderService(SaesContext saesContext, ILogger<KeyHolderService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.KeyHolderLookupResponse> Search(Protos.ModelServices.KeyHolderLookup request, ServerCallContext context)
        {
            var query = _ctx.KeyHolders.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.KeyHolderID != null ? query.Where(x => x.KeyHolderId == request.KeyHolderID) : query;
            query = request.SerialNumber != null ? query.Where(x => x.SerialNumber.Contains(request.SerialNumber)) : query;
            query = request.TypeID != null ? query.Where(x => x.TypeId == request.TypeID) : query;
            query = request.UserCPI != null ? query.Where(x => x.UserCpi == request.UserCPI) : query;

            var response = new KeyHolderLookupResponse();

            var dtos = await query.ProjectToType<Protos.KeyHolderDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<KeyHolderLookupResponse> Add(KeyHolderDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.SerialNumber))
            {
                throw new ArgumentException($"{nameof(request.SerialNumber)} was null or empty");
            }

            if (!request.TypeID.HasValue)
            {
                throw new ArgumentException($"{nameof(request.TypeID)} was null");
            }

            if(await _ctx.KeyHolders.FirstOrDefaultAsync(x => x.SerialNumber == request.SerialNumber) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.SerialNumber));
            }

            var entity = new KeyHolder
            {
                SerialNumber = request.SerialNumber,
                UserCpi = request.UserCPI,
                TypeId = request.TypeID.Value
            };

            entity = (await _ctx.KeyHolders.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new KeyHolderLookupResponse();
            response.Data.Add(entity.Adapt<KeyHolderDto>());

            return response;
        }

        public override async Task<StatusResponse> Edit(KeyHolderDataRequest request, ServerCallContext context)
        {
            if (!request.KeyHolderID.HasValue)
            {
                throw new ArgumentException($"{nameof(request.KeyHolderID)} was null");
            }

            if (string.IsNullOrEmpty(request.SerialNumber))
            {
                throw new ArgumentException($"{nameof(request.SerialNumber)} was null or empty");
            }

            if (!request.TypeID.HasValue)
            {
                throw new ArgumentException($"{nameof(request.TypeID)} was null");
            }

            if (await _ctx.KeyHolders.AnyAsync(x => x.SerialNumber == request.SerialNumber && x.KeyHolderId != request.KeyHolderID))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.SerialNumber));
            }

            var entity = await _ctx.KeyHolders.SingleAsync(x => x.KeyHolderId == request.KeyHolderID);

            entity.SerialNumber = request.SerialNumber;
            entity.TypeId = request.TypeID.Value;
            entity.UserCpi = request.UserCPI.Value;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(KeyHolderLookup request, ServerCallContext context)
        {
            if (!request.KeyHolderID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.KeyHolderID));
            }

            var entity = await _ctx.KeyHolders.SingleAsync(x => x.KeyHolderId == request.KeyHolderID);

            _ctx.KeyHolders.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
