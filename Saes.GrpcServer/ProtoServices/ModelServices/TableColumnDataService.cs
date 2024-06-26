﻿using Grpc.Core;
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
    public class TableColumnDataService : Saes.Protos.ModelServices.TableColumnDataService.TableColumnDataServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<TableColumnDataService> _logger;
        private readonly IMapper _mapper;

        public TableColumnDataService(SaesContext saesContext, ILogger<TableColumnDataService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.TableColumnDataLookupResponse> Search(Protos.ModelServices.TableColumnDataLookup request, ServerCallContext context)
        {
            var query = _ctx.TableColumnData.AsQueryable();

            //query = query.Where(x => x.SysIsDeleted == false);

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.TableColumnDataID != null ? query.Where(x => x.TableColumnDataId == request.TableColumnDataID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new TableColumnDataLookupResponse();

            var entities = await query.ToListAsync();
			
			response.Data.AddRange(entities.Select( x => x.Adapt<TableColumnDataDto>(_mapper.Config)));

            return response;
        }
    }
}
