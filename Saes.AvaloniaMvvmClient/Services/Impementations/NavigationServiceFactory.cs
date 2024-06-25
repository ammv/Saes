using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class NavigationServiceFactory : ReactiveObject, INavigationServiceFactory
    {
        private INavigationService _singletonNavigationService;
        public INavigationService Singleton
        {
            get
            {
                if(_singletonNavigationService == null)
                {
                    this.RaiseAndSetIfChanged(ref _singletonNavigationService, CreateService());
                }
                return _singletonNavigationService;
            }
        }

        public event EventHandler<INavigationService> Created;

        public INavigationService Create()
        {
            NavigationService service = CreateService();
            return service;
        }

        private NavigationService CreateService()
        {
            var service = new NavigationService();
            Created?.Invoke(this, service);
            return service;
        }
    }
}
