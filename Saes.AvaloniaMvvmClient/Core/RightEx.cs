using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public sealed class RightEx: ReactiveObject
    {
        [Reactive]
        public RightDto Right { get; set; }
        public bool Has => Right != null;
        [Reactive]
        public bool Enabled { get; set; }
    }

    
}
