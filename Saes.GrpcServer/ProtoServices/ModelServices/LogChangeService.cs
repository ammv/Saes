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
    public class LogChangeService : Saes.Protos.ModelServices.LogChangeService.LogChangeServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<LogChangeService> _logger;
        private readonly IMapper _mapper;

        public LogChangeService(SaesContext saesContext, ILogger<LogChangeService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.LogChangeLookupResponse> Search(Protos.ModelServices.LogChangeLookup request, ServerCallContext context)
        {
            var query = _ctx.LogChanges.AsQueryable();

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.LogChangeID != null ? query.Where(x => x.LogChangeId == request.LogChangeID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new LogChangeLookupResponse();

            var dtos = await query.ProjectToType<Protos.LogChangeDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }
    }
}
