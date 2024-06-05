using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class NavigationService : ReactiveObject, INavigationService
    {
        private ViewModelBase _content;
        public ViewModelBase Content => _content;

        public void NavigateTo(ViewModelBase content)
        {
            this.RaiseAndSetIfChanged(ref _content, content, nameof(Content));
        }
    }
}
