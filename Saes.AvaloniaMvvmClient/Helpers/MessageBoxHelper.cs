using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Helpers
{
    public static class MessageBoxHelper
    {
        public static async Task<bool> Question(string title, string message, Window window = null)
        {
            window = window ?? WindowManager.Windows.First();

            var result = await MessageBoxManager.GetMessageBoxStandard(
                    title, message,
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowWindowDialogAsync(window);

            return result == MsBox.Avalonia.Enums.ButtonResult.Yes;
        }
    }
}
