using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public class ReactiveCommandEx<TParam, TResult> : ReactiveObject
    {
        private bool _isExecuting;
        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                this.RaiseAndSetIfChanged(ref _isExecuting, value);
            }
        }

        public ReactiveCommand<TParam, TResult> Command { get; }

        public ReactiveCommandEx(ReactiveCommand<TParam, TResult> reactiveCommand)
        {
            Command = reactiveCommand;
            Command.IsExecuting.Subscribe(x => IsExecuting = x);
        }
    }
}
