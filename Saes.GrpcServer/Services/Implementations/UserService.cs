using Saes.GrpcServer.Services.Interfaces;
using Saes.Models;
using Saes.Models.Schemas;

namespace Saes.GrpcServer.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly SaesContext _ctx;

        public UserService(SaesContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<bool> VerifyUserAsync(string username, string password)
        {
            return await _ctx.udfVerifyUserAsync(username, password);
        }

        public bool VerifyUser(string username, string password)
        {
            return _ctx.udfVerifyUser(username, password);
        }
    }
}
