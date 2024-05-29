using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Administration.User
{
    public class UserListViewModel : ViewModelCloseableBase
    {
        public override bool Close()
        {
            var result = MessageBoxManager.GetMessageBoxStandard("Question", "Close this tab?", MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync().GetAwaiter().GetResult();
            return result == MsBox.Avalonia.Enums.ButtonResult.Yes;
        }

        //private int myVar;

        //public int MyProperty
        //{
        //    get { return myVar; }
        //    set { myVar = value; }
        //}

    }
}
