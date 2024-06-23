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
    public class EmployeeService : Saes.Protos.ModelServices.EmployeeService.EmployeeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IMapper _mapper;

        public EmployeeService(SaesContext saesContext, ILogger<EmployeeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.EmployeeLookupResponse> Search(Protos.ModelServices.EmployeeLookup request, ServerCallContext context)
        {
            var query = _ctx.Employees.AsQueryable();

            query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            query = request.EmployeeID != null ? query.Where(x => x.EmployeeId == request.EmployeeID) : query;
            query = request.EmployeePositionID != null ? query.Where(x => x.EmployeePositionId == request.EmployeePositionID) : query;
            query = request.FirstName != null ? query.Where(x => x.FirstName.Contains(request.FirstName)) : query;
            query = request.MiddleName != null ? query.Where(x => x.MiddleName.Contains(request.MiddleName)) : query;
            query = request.LastName != null ? query.Where(x => x.LastName.Contains(request.LastName)) : query;

            query = query
                .Include(x => x.BusinessEntity)
                .Include(x => x.EmployeePosition);

            var response = new EmployeeLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<EmployeeDto>(_mapper.Config)));

            return response;
        }

        public override async Task<EmployeeLookupResponse> Add(EmployeeDataRequest request, ServerCallContext context)
        {

            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeePositionID)} was null"));
            }

            if (!request.EmployeePositionID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeePositionID)} was null"));
            }

            var entity = new Employee
            {
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                OrganizationId = request.OrganizationID.Value,
                EmployeePositionId = request.EmployeePositionID.Value,
                BusinessEntity = new BusinessEntity { BusinessEntityTypeId = 2 }
            };

            entity = (await _ctx.Employees.AddAsync(entity)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new EmployeeLookupResponse();
            response.Data.Add(entity.Adapt<EmployeeDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(EmployeeDataRequest request, ServerCallContext context)
        {
            if (!request.EmployeeID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeeID)} was null or empty"));
            }

            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeePositionID)} was null"));
            }

            if (!request.EmployeePositionID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeePositionID)} was null"));
            }

            var entity = await _ctx.Employees.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.EmployeePositionID}"));
            }


            entity.FirstName = request.FirstName;
            entity.MiddleName = request.MiddleName;
            entity.LastName = request.LastName;
            entity.OrganizationId = request.OrganizationID.Value;
            entity.EmployeePositionId = request.EmployeePositionID.Value;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(EmployeeLookup request, ServerCallContext context)
        {
            if (!request.EmployeeID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.EmployeeID)} was null or empty"));
            }

            var entity = await _ctx.Employees.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.EmployeeID}"));
            }

            _ctx.Employees.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
