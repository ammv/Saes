using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.Authentication.User
{
    public partial class UserFormView : Window
    {
        public UserFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
        }

        ~UserFormView()
        {
            WindowManager.Remove(this);
        }
    }
}
