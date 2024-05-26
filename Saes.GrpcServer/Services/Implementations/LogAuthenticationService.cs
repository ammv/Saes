using Saes.GrpcServer.Services.Interfaces;
using Saes.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Saes.GrpcServer.Services.Implementations
{
    public class LogAuthenticationService : ILogAuthenticationService
    {
        private readonly SaesContext _ctx;

        public LogAuthenticationService(SaesContext ctx)
        {
            _ctx = ctx;
        }
        public void AddLog(string enteredLogin, bool firstFactorResult, bool secondFactorResult, string authServiceResponse)
        {
            _ctx.LogAuthentications.Add(new LogAuthentication
            {
                EnteredLogin = enteredLogin,
                FirstFactorResult = firstFactorResult,
                SecondFactorResult = secondFactorResult,
                AuthServiceResponse = authServiceResponse,
                Date = DateTime.Now
            });
            _ctx.SaveChanges();
        }

        public async Task AddLogAsync(string enteredLogin, bool firstFactorResult, bool secondFactorResult, string authServiceResponse)
        {
            await _ctx.LogAuthentications.AddAsync(new LogAuthentication
            {
                EnteredLogin = enteredLogin,
                FirstFactorResult = firstFactorResult,
                SecondFactorResult = secondFactorResult,
                AuthServiceResponse = authServiceResponse,
                Date = DateTime.Now
            });
            await _ctx.SaveChangesAsync();
        }
    }
}
