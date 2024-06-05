using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.KeyHolderType
{
    public partial class KeyHolderTypeFormView : Window
    {
        public KeyHolderTypeFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
        }

        ~KeyHolderTypeFormView()
        {
            WindowManager.Remove(this);
        }
    }
}
