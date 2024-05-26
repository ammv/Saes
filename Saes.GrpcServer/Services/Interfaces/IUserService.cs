namespace Saes.GrpcServer.Services.Interfaces
{
    public interface IUserService
    {
        public bool VerifyUser(string username, string password);
        public Task<bool> VerifyUserAsync(string username, string password);
    }
}
