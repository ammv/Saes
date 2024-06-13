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
        protected override bool Validate()
        {
            throw new NotImplementedException();
        }

        protected override UserDataRequest _Configure(UserDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new UserDataRequest
                {
                    Login = dto.Login
                };
            }
            else
            {
                return new UserDataRequest { UserId = 0};
            }
            
        }

        protected override void _ConfigureTitle()
        {
            throw new NotImplementedException();
        }

        protected override Task _Loaded()
        {
            throw new NotImplementedException();
        }

        protected override Task _OnAdd()
        {
            throw new NotImplementedException();
        }

        protected override Task _OnEdit()
        {
            throw new NotImplementedException();
        }

        protected override Task _OnPreFormCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task _OnSee()
        {
            throw new NotImplementedException();
        }
    }
}
