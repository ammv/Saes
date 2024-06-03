using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface IDialogService
    {
        //var store = new UserFormViewModel();
        //var dialog = new UserFormView();
        //dialog.DataContext = store;

        //    var result = await dialog.ShowDialog<UserDto?>(WindowManager.Windows.Last());

        public void ShowDialog(ViewModelBase viewModel);
    }
}
