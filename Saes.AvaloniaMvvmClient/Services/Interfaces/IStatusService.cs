using Saes.AvaloniaMvvmClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface IStatusService
    {
        public StatusData Status { get; set;  }

        public void Send(StatusData statusData);
    }
}
