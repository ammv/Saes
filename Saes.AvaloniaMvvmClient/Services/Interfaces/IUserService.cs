using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface IUserService
    {
        public void LoadRights();
        public IReadOnlyCollection<string> GetRights();
    }
}
