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
    public class HardwareService: Protos.ModelServices.HardwareService.HardwareServiceBase
    {
        private readonly ILogger<HardwareService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public HardwareService(ILogger<HardwareService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<HardwareLookupResponse> Search(HardwareLookup request, ServerCallContext context)
        {

            var query = _ctx.Hardwares.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.HardwareId != null ? query.Where(x => x.HardwareId == request.HardwareId) : query;
            query = request.OrganizationID != null ? query.Where(x => x.OrganizationId == request.OrganizationID) : query;
            query = request.SerialNumber != null ? query.Where(x => x.SerialNumber.Contains(request.SerialNumber)) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

           // query = query.Include(x => x.Hardware);

            var response = new HardwareLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<HardwareDto>(_mapper.Config)));

            return response;
        }

        public override async Task<HardwareLookupResponse> Add(HardwareDataRequest request, ServerCallContext context)
        {
            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.OrganizationID)} was null"));
            }

            if(request != null && await _ctx.Hardwares.AnyAsync(x => x.SerialNumber == request.SerialNumber))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.SerialNumber}"));
            }

            var entity = new Hardware
            {
                Name = request.Name,
                OrganizationId = request.OrganizationID,
                SerialNumber = request.SerialNumber,
                Note = request.Note,
            };

            entity = (await _ctx.Hardwares.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new HardwareLookupResponse();
            response.Data.Add(entity.Adapt<HardwareDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(HardwareDataRequest request, ServerCallContext context)
        {
            if (!request.HardwareId.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.HardwareId)} was null or empty"));
            }

            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.OrganizationID)} was null"));
            }

            var entity = await _ctx.Hardwares.FirstOrDefaultAsync(x => x.HardwareId == request.HardwareId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.HardwareId}"));
            }

            if(!string.IsNullOrEmpty(request.SerialNumber) && await _ctx.Hardwares.AnyAsync(x => x.HardwareId != request.HardwareId && x.SerialNumber == request.SerialNumber))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.SerialNumber}"));
            }

            entity.Name = request.Name;
            entity.SerialNumber = request.SerialNumber;
            entity.Note = request.Note;
            entity.OrganizationId = request.OrganizationID.Value;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(HardwareLookup request, ServerCallContext context)
        {
            if (!request.HardwareId.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.HardwareId)} was null or empty"));
            }

            var entity = await _ctx.Hardwares.FirstOrDefaultAsync(x => x.HardwareId == request.HardwareId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.HardwareId}"));
            }

            _ctx.Hardwares.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
