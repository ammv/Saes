using Avalonia.Controls;
using Avalonia.Media;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels.Authentication.User
{
    public class UserFormViewModel : ViewModelFormBase<UserDto, UserDataRequest>
    {
        protected override async Task OnFormCommand()
        {
            if (CurrentMode == Core.Enums.FormMode.Edit)
            {
                UserDto clone = Dto.Clone();

                clone.Login = DataRequest.Login;

                Callback(clone);
            }
            else
            {
                Callback(null);
            }

            await WindowManager.Close((w) => w.DataContext == this);
        }

        protected override void _Configure(UserDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                DataRequest.Login = dto.Login;
            }
        }
    }
}
