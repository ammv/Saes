using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface INavigationService
    {
        public event EventHandler<ViewModelBase> Navigated;
        public ViewModelBase Content { get; }
        public void NavigateTo(ViewModelBase content);
    }
}
