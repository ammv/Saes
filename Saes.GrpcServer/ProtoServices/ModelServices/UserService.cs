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

            var response = new UserLookupResponse();

            var dtos = await _mapper.From(query).ProjectToType<UserDto>().ToListAsync();

            response.Data.AddRange(dtos);

            return response;

            //var query = _mapper.From(_ctx.Users).ProjectToType<UserDto>();

            //query = query.Where(x => x.SysIsDeleted == false);

            //query = request.UserId != null ? query.Where(x => x.UserId == request.UserId) : query; 
            //query = request.Login != null ? query.Where(x => x.Login.Contains(request.Login)) : query; 
            //query = request.UserRoleId != null ? query.Where(x => x.UserRoleId == request.UserRoleId) : query; 
            //query = request.UserRoleName != null ? query.Where(x => x.UserRole.Name.Contains(request.UserRoleName)) : query;

            //var result = await users.ToListAsync();
        }
    }
    //public class UserService: Protos.ModelServices.User.UserBase
    //{
    //    private readonly ILogger<UserService> _logger;
    //    private readonly SaesContext _ctx;

    //    public UserService(ILogger<UserService> logger, SaesContext ctx)
    //    {
    //        _logger = logger;
    //        _ctx = ctx;
    //    }
    //    public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
    //    {
    //        var query = _ctx.Users.AsQueryable();

    //        query = query.Where(x => x.SysIsDeleted == false);

    //        query = request.UserId != null ? query.Where(x => x.UserId == request.UserId) : query; 
    //        query = request.Login != null ? query.Where(x => x.Login.Contains(request.Login)) : query; 
    //        query = request.UserRoleId != null ? query.Where(x => x.UserRoleId == request.UserRoleId) : query; 
    //        query = request.UserRoleName != null ? query.Where(x => x.UserRole.Name.Contains(request.UserRoleName)) : query;

    //        var result = await query.OrderByDescending(x => x.UserId).ToListAsync();

    //        var userModels = new List<UserModel>();

    //        //foreach (var user in result)
    //        //{ 
    //        //   var userModel = new UserModel();
    //        //    userModel.Login = user.Login;
    //        //    userModel.User = user.UserRole;
    //        //    userModel.UserRoleId = user.UserRoleId;
    //        //}

    //        //var response = new SearchResponse();


    //        //response.Result.AddRange(result.Select(x => new Protos.UserModel
    //        //{
    //        //    Login = x.Login,
    //        //    LastLoginDate = Timestamp.FromDateTime(x.LastLoginDate.Value),

    //        //}).ToList());

    //        //return base.Search(request, context);

    //        return null;
    //    }
    //}
}
