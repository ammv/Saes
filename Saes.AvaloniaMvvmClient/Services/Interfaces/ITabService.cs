using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface ITabService
    {
        public event EventHandler<ViewModelTabBase> SelectedTabChanging;
        public event EventHandler<ViewModelTabBase> SelectedTabChanged;
    }
}
