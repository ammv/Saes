using Google.Protobuf.WellKnownTypes;
using Mapster;
using Saes.Models;
using Saes.Protos;

namespace Saes.GrpcServer.Mapping
{
    public class RegisterMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            bool requireDms = false;
            bool preserveRef = true;
            config.NewConfig<Models.File, FileDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<UserRole, UserRoleDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<RightGroup, RightGroupDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<Right, RightDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<UserRoleRight, UserRoleRightDto>()
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
            config.NewConfig<User, UserDto>()
                .Map(dest => dest.LastLoginDate, src => src.LastLoginDate.HasValue ? Timestamp.FromDateTime(src.LastLoginDate.Value.ToUniversalTime()) : null)
                .Map(dest => dest.UserRoleDto, src => src.UserRole)
                .RequireDestinationMemberSource(requireDms)
                .PreserveReference(preserveRef);
        }
    }
}
