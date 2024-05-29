using Mapster;
using Saes.Models;
using Saes.Protos;

namespace Saes.GrpcServer.Mapping
{
    public class RegisterMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Models.File, FileDto>()
                .RequireDestinationMemberSource(true);
            config.NewConfig<UserRole, UserRoleDto>()
                .RequireDestinationMemberSource(true);
            config.NewConfig<RightGroup, RightGroupDto>()
                .RequireDestinationMemberSource(true);
            config.NewConfig<Right, RightDto>()
                .RequireDestinationMemberSource(true);
            config.NewConfig<UserRoleRight, UserRoleRightDto>()
                .RequireDestinationMemberSource(true);
            config.NewConfig<User, UserDto>()
                .RequireDestinationMemberSource(true);
        }
    }
}
