using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public class MessageBoxHelperVm<TViewModelForm> where TViewModelForm: ViewModelFormBase
    {
        private readonly TViewModelForm _viewModel;
        private Window _windowForCurrentVm;
        public MessageBoxHelperVm(TViewModelForm ViewModel)
        {
            _viewModel = ViewModel;
            _windowForCurrentVm = WindowManager.GetByViewModel(_viewModel);
        }

        public async Task<bool> Question(string title, string message, bool isWarning = false)
        {
            return await MessageBoxHelper.Question(title, message, _windowForCurrentVm, isWarning);
        }

        public async Task Information(string title, string message, Window window = null)
        {
            await MessageBoxHelper.Information(title, message, _windowForCurrentVm);
        }

        public async Task Exception(string title, string message, Window window = null)
        {
            await MessageBoxHelper.Exception(title, message, _windowForCurrentVm);
        }

        public async Task Success(string title, string message, Window window = null)
        {
            await MessageBoxHelper.Success(title, message, _windowForCurrentVm);
        }

        public async Task NotImplementedError(Window window = null)
        {
            await MessageBoxHelper.NotImplementedError(_windowForCurrentVm);

        }
    }
}
