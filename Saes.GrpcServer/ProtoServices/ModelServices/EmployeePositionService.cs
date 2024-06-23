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
    public class EmployeePositionService : Saes.Protos.ModelServices.EmployeePositionService.EmployeePositionServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<EmployeePositionService> _logger;
        private readonly IMapper _mapper;

        public EmployeePositionService(SaesContext saesContext, ILogger<EmployeePositionService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.EmployeePositionLookupResponse> Search(Protos.ModelServices.EmployeePositionLookup request, ServerCallContext context)
        {
            var query = _ctx.EmployeePositions.AsQueryable();

            query = request.EmployeePositionID != null ? query.Where(x => x.EmployeePositionId == request.EmployeePositionID) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;
            query = request.Note != null ? query.Where(x => x.Note.Contains(request.Note)) : query;

            var response = new EmployeePositionLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<EmployeePositionDto>(_mapper.Config)));

            return response;
        }

        public override async Task<EmployeePositionLookupResponse> Add(EmployeePositionDataRequest request, ServerCallContext context)
        {
            if(string.IsNullOrEmpty(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Empty name of employee position"));
            }

            var empPos = await _ctx.EmployeePositions.FirstOrDefaultAsync(x => x.Name == request.Name);

            if(empPos != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.Name));
            }

            var entity = new EmployeePosition
            {
                Name = request.Name
            };

            entity = (await _ctx.EmployeePositions.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new EmployeePositionLookupResponse();
            response.Data.Add(entity.Adapt<EmployeePositionDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(EmployeePositionDataRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,$"{nameof(request.Name)} was null or empty"));
            }

            if (!request.EmployeePositionID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeePositionID)} was null"));
            }

            var entity = await _ctx.EmployeePositions.FirstOrDefaultAsync(x => x.EmployeePositionId == request.EmployeePositionID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.EmployeePositionID}"));
            }

            if(await _ctx.EmployeePositions.AnyAsync(x => x.EmployeePositionId != request.EmployeePositionID && x.Name == request.Name))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.Name}"));
            }

            entity.Name = request.Name;
            entity.Note = request.Note;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(EmployeePositionLookup request, ServerCallContext context)
        {
            if (!request.EmployeePositionID.HasValue)
            {
                throw new ArgumentNullException(nameof(request.EmployeePositionID));
            }

            var entity = await _ctx.EmployeePositions.FirstOrDefaultAsync(x => x.EmployeePositionId == request.EmployeePositionID);

            if(entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.EmployeePositionID}"));
            }

            _ctx.EmployeePositions.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
