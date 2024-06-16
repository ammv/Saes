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

            var response = new EmployeeLookupResponse();

            var dtos = await query.ProjectToType<Protos.EmployeeDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<EmployeeLookupResponse> Add(EmployeeDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(EmployeeDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(EmployeeLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
