using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface INavigationServiceFactory
    {
        public event EventHandler<INavigationService> Created;
        public INavigationService Create();
        public INavigationService Singleton { get; }
    }
}
