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
    public class ErrorLogService : Saes.Protos.ModelServices.ErrorLogService.ErrorLogServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<ErrorLogService> _logger;
        private readonly IMapper _mapper;

        public ErrorLogService(SaesContext saesContext, ILogger<ErrorLogService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.ErrorLogLookupResponse> Search(Protos.ModelServices.ErrorLogLookup request, ServerCallContext context)
        {
            var query = _ctx.ErrorLogs.AsQueryable();

            if(request.ErrorLogID != null)
            {
                query = query.Where(x => x.ErrorLogId == request.ErrorLogID);
                goto EndFilters;
            }

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.ErrorLogID != null ? query.Where(x => x.ErrorLogId == request.ErrorLogID) : query;

            EndFilters:

            //uery = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new ErrorLogLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<ErrorLogDto>(_mapper.Config)));

            return response;
        }
    }
}
