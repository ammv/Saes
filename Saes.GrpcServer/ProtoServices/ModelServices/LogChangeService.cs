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

            if(request.AuditLogId != null)
            {
                query = query.Where(x => x.AuditLogId == request.AuditLogId);
                goto EndFilters;
            }

            query = request.TableColumnDataId != null ? query.Where(x => x.TableColumnDataId == request.TableColumnDataId) : query;
            query = request.OldValue != null ? query.Where(x => x.OldValue.Contains(request.OldValue)) : query;
            query = request.NewValue != null ? query.Where(x => x.NewValue.Contains(request.NewValue)) : query;

            EndFilters:

            query = query
                .Include(x => x.AuditLog)
                .Include(x => x.TableColumnData);

            var response = new LogChangeLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<LogChangeDto>(_mapper.Config)));

            return response;
        }
    }
}
