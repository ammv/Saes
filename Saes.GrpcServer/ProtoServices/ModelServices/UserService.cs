using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OtpNet;
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

            var entities = await query.ToListAsync();
			
			response.Data.AddRange(entities.Select( x => x.Adapt<UserDto>(_mapper.Config)));

            return response;
        }

        public override async Task<UserLookupResponse> Add(UserDataRequest request, ServerCallContext context)
        {
            //_ctx.uspAddUser

            if(string.IsNullOrEmpty(request.Login))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан логин"));
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан пароль"));
            }

            if (request.UserRoleId == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указана роль пользователя"));
            }

            User foundByLogin = await _ctx.Users.FirstOrDefaultAsync(x => x.Login == request.Login);
            if(foundByLogin != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Пользователь с таким логином уже существует"));
            }

            await _ctx.uspAddUserAsync(request.Login, request.Password, request.UserRoleId.Value);

            //await _ctx.SaveChangesAsync();

            var response = new UserLookupResponse();

            var addedUser = await _ctx.Users.SingleAsync(x => x.Login == request.Login);

            addedUser.TwoFactorEnabled = request.TwoFactorEnabled;

            await _ctx.SaveChangesAsync();

            response.Data.Add(addedUser.Adapt<UserDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(UserDataRequest request, ServerCallContext context)
        {
            if(request.UserId == null || request.UserId <= 0)
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            if(string.IsNullOrEmpty(request.Password))

            if(await _ctx.Users.FirstOrDefaultAsync(x => x.Login == request.Login && x.UserId != request.UserId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, request.Login));
            }

            User user = await _ctx.Users.SingleAsync(x => x.UserId == request.UserId);

            user.Login = request.Login;
            user.UserRoleId = request.UserRoleId;

            if(!string.IsNullOrEmpty(request.Password))
            {
                await _ctx.uspUpdatePasswordUserAsync(request.Login, request.Password);
            }

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
            
        }

        public override async Task<StatusResponse> Remove(UserLookup request, ServerCallContext context)
        {
            if(request.UserId == null)
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }
            User user = await _ctx.Users.SingleAsync(x => x.UserId == request.UserId);
            _ctx.Users.Remove(user);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<UserGetRightsResponse> GetRights(UserGetRightsRequest request, ServerCallContext context)
        {
            if (request.UserId == null || request.UserId <= 0)
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            User user = await _ctx.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);

            if(user == null)
            {
                throw new ArgumentNullException(nameof(request.UserId));
            }

            //List<UserRoleRight> userRoleRights = _ctx.UserRoleRights.Where(x => x.UserRoleId == user.UserRoleId).ToList();

            List<string> rights = await _ctx.UserRoleRights.Where(x => x.UserRoleId == user.UserRoleId).Select(x => x.Right.Code).ToListAsync();

            var response = new UserGetRightsResponse();

            response.Data.AddRange(rights);

            return response;


        }

        public override async Task<UpdateTwoFactorTokenResponse> UpdateTwoFactorToken(UserLookup request, ServerCallContext context)
        {
            if (request.UserId == null || request.UserId <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе не был указан идентификатор пользователя или он был неккоректный"));
            }

            User user = await _ctx.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"В запросе не был указан несуществующий идентификатор пользователя"));
            }

            if(user.TwoFactorEnabled.HasValue && user.TwoFactorEnabled.Value != true)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Вы не можете получить токен OTP для пользователя, у которого не включена двухфакторная аутенфикация"));
            }

            byte[] token = KeyGeneration.GenerateRandomKey(20);

            var strToken = Base32Encoding.ToString(token);

            var strUriToken = new OtpUri(OtpType.Totp, strToken, user.Login, "СУЭП").ToString();

            user.TotpSecretKey = strToken;

            await _ctx.SaveChangesAsync();

            return new UpdateTwoFactorTokenResponse { Token = strToken, UriToken = strUriToken };

        }
    }
}
