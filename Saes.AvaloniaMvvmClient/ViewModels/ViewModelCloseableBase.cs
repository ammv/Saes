using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public abstract class ViewModelCloseableBase: ViewModelBase
    {
        protected bool IsForceClose { get; private set; } = false;
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
                _close = () => { IsForceClose = true; value(); };
            }
        }

        [Reactive]
        public ReactiveCommand<WindowClosingEventArgs, Unit> ClosingCommand { get; private set; }

        public ViewModelCloseableBase()
        {
            ClosingCommand = ReactiveCommand.CreateFromTask<WindowClosingEventArgs>(OnClosingCommand);
        }

        protected abstract Task OnClosingCommand(WindowClosingEventArgs closingEventArgs);

    }
}
