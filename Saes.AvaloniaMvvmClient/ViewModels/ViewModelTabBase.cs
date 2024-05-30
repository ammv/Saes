using Avalonia.Input;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public abstract class ViewModelTabBase: ViewModelBase
    {
        protected string _tabTitle;
        public string TabTitle
        {
            get => _tabTitle;
            set => this.RaiseAndSetIfChanged(ref _tabTitle, value);
        }
        public abstract Task<bool> CloseAsync();
    }
}
