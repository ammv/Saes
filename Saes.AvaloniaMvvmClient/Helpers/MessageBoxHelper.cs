using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Helpers
{
    public static class MessageBoxHelper
    {
        private const double _messageBoxWidth = 500;
        private const WindowStartupLocation _windowStartupLocation = WindowStartupLocation.CenterOwner;
        private static MessageBoxStandardParams _GetMessageBoxStandardParams(string title, string text)
        {
            return new MessageBoxStandardParams 
            { 
                MaxWidth = _messageBoxWidth,
                ContentTitle = title, ContentMessage = text,
                WindowStartupLocation = _windowStartupLocation };
        }

        private static async Task<ButtonResult> _ShowWindowDialogAsync(MessageBoxStandardParams p, Window window)
        {
            return await MessageBoxManager.GetMessageBoxStandard(p).ShowWindowDialogAsync(window);
        }

        public static async Task<bool> Question(string title, string message, Window window = null, bool isWarning = false)
        {
            window = window ?? WindowManager.Windows.First();

            var p = _GetMessageBoxStandardParams(title, message);
            p.ButtonDefinitions = ButtonEnum.YesNo;
            p.Icon = isWarning ? Icon.Warning: Icon.Question;

            return await _ShowWindowDialogAsync(p, window) == ButtonResult.Yes;
        }

        public static async Task Information(string title, string message, Window window = null)
        {
            window = window ?? WindowManager.Windows.First();

            var p = _GetMessageBoxStandardParams(title, message);
            p.ButtonDefinitions = ButtonEnum.Ok;
            p.Icon = Icon.Info;

           await _ShowWindowDialogAsync(p, window);
        }

        public static async Task Exception(string title, string message, Window window = null)
        {
            window = window ?? WindowManager.Windows.First();

            var p = _GetMessageBoxStandardParams(title, message);
            p.ButtonDefinitions = ButtonEnum.Ok;
            p.Icon = Icon.Error;

            await _ShowWindowDialogAsync(p, window);
        }

        public static async Task Success(string title, string message, Window window = null)
        {
            window = window ?? WindowManager.Windows.First();

            var p = _GetMessageBoxStandardParams(title, message);
            p.ButtonDefinitions = ButtonEnum.Ok;
            p.Icon = Icon.Success;

            await _ShowWindowDialogAsync(p, window);
        }

        public static async Task NotImplementedError(Window window = null)
        {
            window = window ?? WindowManager.Windows.First();

            var p = _GetMessageBoxStandardParams("Ошибка", "Данная функция ещё не доступна");
            p.ButtonDefinitions = ButtonEnum.Ok;
            p.Icon = Icon.Error;

            await _ShowWindowDialogAsync(p, window);
        }
    }
}
