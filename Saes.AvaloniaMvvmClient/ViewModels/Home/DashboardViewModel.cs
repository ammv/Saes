using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Home
{
    public class DashboardViewModel : ViewModelTabBase
    {
        public DashboardViewModel()
        {
            
        }
        [Reactive]
        public string Hello { get; private set; }
        public override Task<bool> CloseAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task _Loaded()
        {
            Hello = "Hello User!";
            await Task.CompletedTask;
        }
    }
}
