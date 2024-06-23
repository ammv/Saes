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
    public class LogService : Saes.Protos.ModelServices.LogService.LogServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<LogService> _logger;
        private readonly IMapper _mapper;

        public LogService(SaesContext saesContext, ILogger<LogService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.LogLookupResponse> Search(Protos.ModelServices.LogLookup request, ServerCallContext context)
        {
            var query = _ctx.Logs.AsQueryable();

            query = request.LogID != null ? query.Where(x => x.LogId == request.LogID) : query;
            if(request.LogID != null)
            {
                query = query.Where(x => x.LogId == request.LogID);
                goto EndFilters;
            }

            query = request.Action != null ? query.Where(x => x.Action == request.Action) : query;
            query = request.GUID != null ? query.Where(x => x.Guid.ToString() == request.GUID) : query;
            query = request.TableDataID != null ? query.Where(x => x.TableDataId == request.TableDataID) : query;
            query = request.TableRowID != null ? query.Where(x => x.TableRowId == request.TableRowID) : query;
            query = request.DateStart != null ? query.Where(x => x.Date >= request.DateStart.ToDateTime().ToLocalTime()) : query;
            query = request.DateEnd != null ? query.Where(x => x.Date <= request.DateEnd.ToDateTime().ToLocalTime()) : query;
            query = request.UserLogin != null ? query.Where(x => x.UserSession.User.Login.Contains(request.UserLogin)) : query;

            EndFilters:

            query = query
                .Include(x => x.TableData)
                .Include(x => x.UserSession)
                    .ThenInclude(x => x.User);


            var response = new LogLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select( x => x.Adapt<LogDto>(_mapper.Config)));

            return response;
        }
    }
}
