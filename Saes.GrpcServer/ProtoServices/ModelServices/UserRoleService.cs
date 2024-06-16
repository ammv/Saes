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

        public override async Task<UserRoleLookupResponse> Add(UserRoleDataRequest request, ServerCallContext context)
        {
            if(string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            UserRole userRole = new UserRole
            {
                Name = request.Name
            };

            userRole = (await _ctx.UserRoles.AddAsync(userRole)).Entity;

            await _ctx.SaveChangesAsync();

            var response = new UserRoleLookupResponse();
            response.Data.Add(userRole.Adapt<UserRoleDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(UserRoleDataRequest request, ServerCallContext context)
        {
            if(string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException($"{nameof(request.Name)} was null or empty");
            }

            if(!request.UserRoleId.HasValue)
            {
                throw new ArgumentNullException(nameof(request.UserRoleId));
            }

            UserRole userRole = await _ctx.UserRoles.SingleAsync(x => x.UserRoleId == request.UserRoleId);

            if(await _ctx.UserRoles.AnyAsync(x => x.Name == request.Name && x.UserRoleId != userRole.UserRoleId))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"{request.Name}"));
            }

            userRole.Name = request.Name;

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(UserRoleLookup request, ServerCallContext context)
        {
            if (!request.UserRoleId.HasValue)
            {
                throw new ArgumentNullException(nameof(request.UserRoleId));
            }

            UserRole userRole = await _ctx.UserRoles.SingleAsync(x => x.UserRoleId == request.UserRoleId);

            _ctx.UserRoles.Remove(userRole);

            return new StatusResponse { Result = true };
        }
    }
}
