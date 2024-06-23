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
    public sealed class RightGroupEx: ReactiveObject
    {
        [Reactive]
        public RightGroupDto RightGroup { get; set; }

        [Reactive]
        public RightEx AddRight { get; set; }
        [Reactive]
        public RightEx SeeRight { get; set; }
        [Reactive]
        public RightEx DeleteRight { get; set; }
        [Reactive]
        public RightEx EditRight { get; set; }
        [Reactive]
        public RightEx ExportRight { get; set; }
    }
}
