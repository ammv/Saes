using Saes.GrpcServer.Services.Implementations;
using Saes.GrpcServer.Services.Interfaces;

namespace Saes.GrpcServer.Services
{
    public class ServicesRegister
    {
        public void Register(IServiceCollection services)
        {
            services.AddScoped<ILogAuthenticationService, LogAuthenticationService>();
            services.AddSingleton<ITokenService, TokenService>();
        }
    }
}
