using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Schemas;
using Saes.Protos;
using Saes.Protos.ModelServices;

#nullable disable

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

        public override async Task<UserLookupResponse> Add(UserDataRequest request, ServerCallContext context)
        {
            //_ctx.uspAddUser

            if(string.IsNullOrEmpty(request.Login))
            {
                throw new ArgumentNullException(nameof(request.Login));
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password));
            }

            User foundByLogin = _ctx.Users.FirstOrDefault(x => x.Login == request.Login);
            if(foundByLogin != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.Login));
            }

            await _ctx.uspAddUserAsync(request.Login, request.Password, request.UserRoleId);

            var response = new UserLookupResponse();

            response.Data.Add(_ctx.Users.Single(x => x.Login == request.Login).Adapt<UserDto>());

            return response;
        }

        public override Task<StatusResponse> Edit(UserDataRequest request, ServerCallContext context)
        {
            if(request.UserId == null || request.UserId <= 0)
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            if(string.IsNullOrEmpty(request.Password))

            if(_ctx.Users.FirstOrDefault(x => x.Login == request.Login && x.UserId != request.UserId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.Login));
            }

            User user = _ctx.Users.Single(x => x.UserId == request.UserId);

            user.Login = request.Login;
            user.UserRoleId = request.UserRoleId;

            _ctx.uspUpdatePasswordUser(request.Login, request.Password);

            _ctx.SaveChanges();

            return Task.FromResult(new StatusResponse { Result = true });
            
        }
    }
}
