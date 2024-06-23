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
            Closed += (s, e) => WindowManager.Remove(this);
        }
    }
}
