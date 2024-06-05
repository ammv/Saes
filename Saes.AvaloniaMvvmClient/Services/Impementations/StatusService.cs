using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class StatusService : ReactiveObject, IStatusService
    {
        private StatusData _statusData;
        public StatusData Status
        { 
            get => _statusData;
            set => this.RaiseAndSetIfChanged(ref _statusData, value);
        }

        public void Send(StatusData statusData)
        {
            MessageBus.Current.SendMessage(statusData);
        }

        public StatusService()
        {
            MessageBus.Current.Listen<StatusData>().Subscribe((x) => Status = x);
        }
    }
}
