using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class UserService: Protos.ModelServices.UserService.UserServiceBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public UserService(ILogger<UserService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<UserLookupResponse> Search(UserLookup request, ServerCallContext context)
        {

            var query = _ctx.Users.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.UserId != null ? query.Where(x => x.UserId == request.UserId) : query;
            query = request.Login != null ? query.Where(x => x.Login.Contains(request.Login)) : query;
            query = request.UserRoleId != null ? query.Where(x => x.UserRoleId == request.UserRoleId) : query;
            query = request.UserRoleName != null ? query.Where(x => x.UserRole.Name.Contains(request.UserRoleName)) : query;

            query = query.Include(x => x.UserRole);

            var response = new UserLookupResponse();

            var dtos = await query.ProjectToType<UserDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }
    }
}
