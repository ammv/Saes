using Google.Protobuf.WellKnownTypes;
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
    public class UserSessionService : Saes.Protos.ModelServices.UserSessionService.UserSessionServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<UserSessionService> _logger;
        private readonly IMapper _mapper;

        public UserSessionService(SaesContext saesContext, ILogger<UserSessionService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.UserSessionLookupResponse> Search(Protos.ModelServices.UserSessionLookup request, ServerCallContext context)
        {
            var query = _ctx.UserSessions.AsQueryable();

            //query = query.Where(x => x.SysIsDeleted == false);

            //query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            //query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            //query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            //query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            //query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            //query = request.UserSessionID != null ? query.Where(x => x.UserSessionId == request.UserSessionID) : query;

            //query = query.Include(x => x.BusinessAddress)
            //    .Include(x => x.BusinessEntity);

            var response = new UserSessionLookupResponse();

            var entities = await query.ToListAsync();
			
			response.Data.AddRange(entities.Select( x => x.Adapt<UserSessionDto>(_mapper.Config)));

            return response;
        }

        public override async Task<GetUserByCurrentSessionResponse> GetUserByCurrentSession(GetUserByCurrentSessionRequest request, ServerCallContext context)
        {
            string? sessionKey = context.RequestHeaders.GetValue("SessionKey");
            UserSession userSession = await _ctx.UserSessions.Include(x => x.User).SingleAsync(x => x.SessionKey == sessionKey);
            return new GetUserByCurrentSessionResponse { User = userSession.User.Adapt<UserDto>(_mapper.Config) };
        }
    }
}
