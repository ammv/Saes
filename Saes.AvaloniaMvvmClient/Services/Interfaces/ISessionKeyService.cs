using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface ISessionKeyService
    {
        string GetSessionKey();
        void SaveSessionKey(string sessionKey);
    }
}
