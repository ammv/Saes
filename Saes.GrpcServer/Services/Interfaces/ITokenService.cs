namespace Saes.GrpcServer.Services.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken();
        public string GenerateToken(int length);
    }
}
