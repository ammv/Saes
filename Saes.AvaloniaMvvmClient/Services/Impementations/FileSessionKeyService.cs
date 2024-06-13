using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class FileSessionKeyService : ISessionKeyService
    {
        private string _sessionKey = null;
        private bool _changedSessionKey = false;
        private const string _sessionKeyFileName = ".session";
        public string GetSessionKey()
        {
            if (!File.Exists(_sessionKeyFileName))
            {
                return null;
            }
            if(_sessionKey != null && _changedSessionKey == false)
            {
                return _sessionKey;
            }
            string sessionKey = _sessionKey = File.ReadAllText(_sessionKeyFileName);
            return sessionKey;
        }

        public void SaveSessionKey(string sessionKey)
        {
            File.WriteAllText(_sessionKeyFileName, sessionKey);
            _changedSessionKey = true;
        }

        public void RemoveSessionKey()
        {
            _sessionKey = null;
            if (File.Exists(_sessionKeyFileName))
            {
                File.Delete(_sessionKeyFileName);
            }
        }
    }
}
