using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class DialogService : IDialogService
    {
        public async void ShowDialog(ViewModelBase viewModel)
        {
            Window dialog = Build(viewModel);

            dialog.DataContext = viewModel;

            await dialog.ShowDialog(WindowManager.Windows.First());
        }

        public Window Build(object data)
        {
            var name = data.GetType().FullName.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Window)Activator.CreateInstance(type);
            }
            else
            {
                throw new Exception($"Not found Window of type {name}");
            }
        }
    }
}
