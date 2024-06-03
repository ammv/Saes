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

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.EmployeeID != null ? query.Where(x => x.EmployeeId == request.EmployeeID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new EmployeeLookupResponse();

            var dtos = await query.ProjectToType<Protos.EmployeeDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }
    }
}
