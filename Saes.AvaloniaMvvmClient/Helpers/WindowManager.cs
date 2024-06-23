using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Helpers
{
    public static class WindowManager
    {
        private static readonly List<Window> _windows = new List<Window>();

        public static IReadOnlyCollection<Window> Windows => _windows;

        public static void Add(Window window)
        {
            if(window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
            _windows.Add(window);
        }

        public static void Remove(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
            _windows.Remove(window);
        }
        
        public static async Task Close(Func<Window, bool> selector)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _windows.FirstOrDefault(selector)?.Close();
            }); 
            
        }

        public static async Task CloseWithException(Func<Window, bool> selector, string errorTitle = null, string errorMessage = null)
        {
            if (errorTitle != null || errorMessage != null)
            {
                await MessageBoxHelper.Exception(errorTitle, errorMessage);
            }
            await Close(selector);
        }
    }
}
