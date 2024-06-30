using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Home
{
    public class DashboardViewModel : ViewModelTabBase
    {
        private List<double> _someBigData = new List<double>();
        public DashboardViewModel()
        {
            TabTitle = "Дэшбоард";
        }
        [Reactive]
        public string Hello { get; private set; }

        protected override async Task _Loaded()
        {
            Hello = string.Empty;
            string text = "Добро пожаловать в СУЭП!";
            foreach(var c in text)
            {
                Hello = $"{Hello}{c}";
                await Task.Delay(75);
            }
            for (double i = 0; i < 3000; i++)
            {
                _someBigData.Add(i);
            }
        }

        ~DashboardViewModel()
        {
            Debug.WriteLine("DashboardViewModel destructed");
        }
    }
}
