using System.Collections.Generic;

namespace Saes.GrpcServer.Services.Interfaces
{
    public interface ILogAuthenticationService
    {
        /// <summary>
        /// Adding authentication log to database
        /// </summary>
        /// <param name="enteredLogin"></param>
        /// <param name="firstFactorResult"></param>
        /// <param name="secondFactorResult"></param>
        /// <param name="authServiceResponse"></param>
        public void AddLog(string enteredLogin, bool firstFactorResult, bool secondFactorResult, string authServiceResponse);
        /// <summary>
        /// Adding authentication log to database. SaveChangedAsync called here
        /// </summary>
        /// <param name="enteredLogin"></param>
        /// <param name="firstFactorResult"></param>
        /// <param name="secondFactorResult"></param>
        /// <param name="authServiceResponse"></param>
        public Task AddLogAsync(string enteredLogin, bool firstFactorResult, bool secondFactorResult, string authServiceResponse);
    }
}
