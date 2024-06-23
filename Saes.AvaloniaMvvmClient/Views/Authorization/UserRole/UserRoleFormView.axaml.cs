using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.Protos;

namespace Saes.AvaloniaMvvmClient.Views.Authorization.UserRole
{
    public partial class UserRoleFormView : Window
    {
        public UserRoleFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s, e) => WindowManager.Remove(this);
        }
    }
}
