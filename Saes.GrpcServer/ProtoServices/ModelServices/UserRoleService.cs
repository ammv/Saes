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
    public class UserRoleService: Protos.ModelServices.UserRoleService.UserRoleServiceBase
    {
        private readonly ILogger<UserRoleService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public UserRoleService(ILogger<UserRoleService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<UserRoleLookupResponse> Search(UserRoleLookup request, ServerCallContext context)
        {

            var query = _ctx.UserRoles.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.UserRoleId != null ? query.Where(x => x.UserRoleId == request.UserRoleId) : query;
            query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

           // query = query.Include(x => x.UserRole);

            var response = new UserRoleLookupResponse();

            var dtos = await query.ProjectToType<UserRoleDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }
    }
}
