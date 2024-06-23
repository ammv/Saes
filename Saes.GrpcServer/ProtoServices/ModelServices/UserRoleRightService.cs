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
    public class UserRoleRightService : Protos.ModelServices.UserRoleRightService.UserRoleRightServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<UserRoleRightService> _logger;
        private readonly IMapper _mapper;

        public UserRoleRightService(SaesContext saesContext, ILogger<UserRoleRightService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<UserRoleRightLookupResponse> Search(UserRoleRightLookup request, ServerCallContext context)
        {
            var query = _ctx.UserRoleRights.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            if (request.UserRoleRightId != null)
            {
                query = query.Where(x => x.UserRoleRightId == request.UserRoleRightId);
                goto EndFilters;
            }

            query = request.UserRoleId != null ? query.Where(x => x.UserRoleId == request.UserRoleId) : query;
            query = request.RightId != null ? query.Where(x => x.RightId == request.RightId) : query;

            EndFilters:

            query = query
                .Include(x => x.Right)
                .Include(x => x.UserRole);

            var response = new UserRoleRightLookupResponse();

            var entities = await query.ToListAsync();
			
			response.Data.AddRange(entities.Select( x => x.Adapt<UserRoleRightDto>(_mapper.Config)));

            return response;
        }

        public override async Task<UserRoleRightLookupResponse> Add(UserRoleRightDataRequest request, ServerCallContext context)
        {
            if (!request.UserRoleId.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан идентификатор роли"));
            }

            if (!await _ctx.UserRoles.AnyAsync(x => x.UserRoleId == request.UserRoleId))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "В запросе был указан несуществующий идентификатор роли"));
            }

            if (!request.RightId.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан идентификатор права"));
            }

            if (!await _ctx.Rights.AnyAsync(x => x.RightId == request.RightId))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "В запросе был указан несуществующий идентификатор права"));
            }

            var response = new UserRoleRightLookupResponse();

            var existingUserRoleRight = await _ctx.UserRoleRights.FirstOrDefaultAsync(x => x.UserRoleId == request.UserRoleId && x.RightId == request.RightId);

            if (existingUserRoleRight != null)
            {
                if (existingUserRoleRight.SysIsDeleted)
                {
                    existingUserRoleRight.SysIsDeleted = false;
                    goto Save;
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Данная роль уже имеет данное право"));
                }
            }

            UserRoleRight userRole = new()
            {
                UserRoleId = request.UserRoleId.Value,
                RightId = request.RightId.Value
            };

            existingUserRoleRight = _ctx.UserRoleRights.Add(userRole).Entity;

            Save:

            await _ctx.SaveChangesAsync();

            response.Data.Add(existingUserRoleRight.Adapt<UserRoleRightDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Remove(UserRoleRightLookup request, ServerCallContext context)
        {
            if (!request.UserRoleRightId.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан идентификатор права роли"));
            }

            var entity = await _ctx.UserRoleRights.FirstOrDefaultAsync(x => x.UserRoleRightId == request.UserRoleRightId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "В запросе был указан несуществующий идентификатор права роли"));
            }

            _ctx.UserRoleRights.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> AddBulk(UserRoleRightBulkRequest request, ServerCallContext context)
        {
            foreach (var item in request.Data)
            {
                if (!item.UserRoleId.HasValue)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан идентификатор роли"));
                }

                if (!await _ctx.UserRoles.AnyAsync(x => x.UserRoleId == item.UserRoleId))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"В запросе был указан несуществующий идентификатор роли: {item.UserRoleId}"));
                }

                if (!item.RightId.HasValue)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан идентификатор права"));
                }

                if (!await _ctx.Rights.AnyAsync(x => x.RightId == item.RightId))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"В запросе был указан несуществующий идентификатор права: {item.RightId}"));
                }
            }

            List<UserRoleRight> addUserRoleRight = new List<UserRoleRight>();

            foreach (var item in request.Data)
            {
                var existingUserRoleRight = await _ctx.UserRoleRights.FirstOrDefaultAsync(x => x.UserRoleId == item.UserRoleId && x.RightId == item.RightId);

                if (existingUserRoleRight != null && existingUserRoleRight.SysIsDeleted)
                {
                    existingUserRoleRight.SysIsDeleted = false;
                }
                else
                {
                    addUserRoleRight.Add(
                    new()
                    {
                        UserRoleId = item.UserRoleId.Value,
                        RightId = item.RightId.Value
                    });
                }
            }

            await _ctx.UserRoleRights.AddRangeAsync(addUserRoleRight);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> RemoveBulk(UserRoleRightBulkRequest request, ServerCallContext context)
        {
            List<UserRoleRight> removeUserRoleRights = await _ctx.UserRoleRights
                .Where(x => request.Data.Select(item => item.UserRoleRightId).Contains(x.UserRoleRightId))
                .ToListAsync();

            if (removeUserRoleRights.Count != request.Data.Count)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "В запросе были указаны несуществующие идентификаторы права ролей"));
            }

            _ctx.UserRoleRights.RemoveRange(removeUserRoleRights.Where( x => !x.SysIsDeleted));

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}