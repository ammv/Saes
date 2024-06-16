using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public abstract class ViewModelCloseableBase: ViewModelBase
    {
        private Action _close;
        public Action Close
        {
            get => _close;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _close = value;
            }
        }
    }
}
