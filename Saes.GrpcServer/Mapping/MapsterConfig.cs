using Mapster;
using Saes.Models;
using Saes.Protos;

namespace Saes.GrpcServer.Mapping
{
    public class MapsterConfig
    {
        public MapsterConfig()
        {
            //TypeAdapterConfig<UserDto, User>.NewConfig()
            //    .Map(dest => dest.UserId, src => src.Id)
            //    .Map(dest => dest.UserName, src => src.Name)
            //    .Map(dest => dest.UserEmail, src => src.Email);
        }
    }
}
