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
    public class TableDataService : Saes.Protos.ModelServices.TableDataService.TableDataServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<TableDataService> _logger;
        private readonly IMapper _mapper;

        public TableDataService(SaesContext saesContext, ILogger<TableDataService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.TableDataLookupResponse> Search(Protos.ModelServices.TableDataLookup request, ServerCallContext context)
        {
            IQueryable<TableDatum> query = ProcessQuery(request);

            var response = new TableDataLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<TableDataDto>(_mapper.Config)));

            return response;
        }

        private IQueryable<TableDatum> ProcessQuery(TableDataLookup request)
        {
            var query = _ctx.TableData.AsQueryable();

            if (request.TableDataId != null)
            {
                query = query.Where(x => x.TableDataId == request.TableDataId);
                goto EndFilters;
            }

            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;
            query = request.RusName != null ? query.Where(x => x.RusName.Contains(request.RusName)) : query;
            query = request.SchemaName != null ? query.Where(x => x.SchemaName.Contains(request.SchemaName)) : query;

            EndFilters:

            query = query.Include(x => x.TableColumnData);
            return query;
        }

        public override async Task<TableDataLookupIncludeColumnsResponse> SearchIncludeColumns(TableDataLookup request, ServerCallContext context)
        {
            IQueryable<TableDatum> query = ProcessQuery(request);

            var response = new TableDataLookupIncludeColumnsResponse();

            var entities = await query.ToListAsync();

            foreach (var entity in entities)
            {
                var dto = entity.Adapt<TableDataDto>(_mapper.Config);
                var tableDataLookupWithColumnsData = new TableDataLookupWithColumnsData();
                tableDataLookupWithColumnsData.Table = dto;
                tableDataLookupWithColumnsData.TableColumns.AddRange(entity.TableColumnData.Select(x => x.Adapt<TableColumnDataDto>(_mapper.Config)));
                response.Data.Add(tableDataLookupWithColumnsData);
            }

            return response;
        }
    }
}
