using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class WindowStateService : ReactiveObject, IWindowStateService
    {
        [Reactive]
        public WindowState State { get; set; }
    }
}
