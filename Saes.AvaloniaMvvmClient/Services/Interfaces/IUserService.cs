using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    internal interface IUserService
    {
        public UserDto CurrentUser { get; set; }

        public bool HasRight(string rightCode);
        public void UpdateCache();
    }
}
