using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.KeyHolder
{
    public partial class KeyHolderFormView : Window
    {
        public KeyHolderFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s, e) => WindowManager.Remove(this);
        }
    }
}
