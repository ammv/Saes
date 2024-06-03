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

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.EmployeePositionID != null ? query.Where(x => x.EmployeePositionId == request.EmployeePositionID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new EmployeePositionLookupResponse();

            var dtos = await query.ProjectToType<Protos.EmployeePositionDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }
    }
}