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
    public class LogAuthenticationService : Saes.Protos.ModelServices.LogAuthenticationService.LogAuthenticationServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<LogAuthenticationService> _logger;
        private readonly IMapper _mapper;

        public LogAuthenticationService(SaesContext saesContext, ILogger<LogAuthenticationService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.LogAuthenticationLookupResponse> Search(Protos.ModelServices.LogAuthenticationLookup request, ServerCallContext context)
        {
            var query = _ctx.LogAuthentications.AsQueryable();

            if (request.LogAuthenticationID != null)
            {
                query = query.Where(x => x.LogAuthenticationId == request.LogAuthenticationID);
                goto EndFilters;
            }

            query = request.AuthServiceResponse != null ? query.Where(x => x.AuthServiceResponse.Contains(request.AuthServiceResponse)) : query;
            query = request.EnteredLogin != null ? query.Where(x => x.EnteredLogin.Contains(request.EnteredLogin)) : query;
            query = request.IP != null ? query.Where(x => x.Ip.Contains(request.IP)) : query;
            query = request.MAC != null ? query.Where(x => x.Mac.Contains(request.MAC)) : query;
            query = request.MashineUserName != null ? query.Where(x => x.MashineUserName.Contains(request.MashineUserName)) : query;
            query = request.FirstFactorResult != null ? query.Where(x => x.FirstFactorResult == x.FirstFactorResult) : query;
            query = request.SecondFactorResult != null ? query.Where(x => x.SecondFactorResult == x.SecondFactorResult) : query;
            query = request.DateStart != null ? query.Where(x => x.Date >= request.DateStart.ToDateTime().ToLocalTime()) : query;
            query = request.DateEnd != null ? query.Where(x => x.Date <= request.DateEnd.ToDateTime().ToLocalTime()) : query;

            EndFilters:

            var response = new LogAuthenticationLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<LogAuthenticationDto>(_mapper.Config)));

            return response;
        }
    }
}
