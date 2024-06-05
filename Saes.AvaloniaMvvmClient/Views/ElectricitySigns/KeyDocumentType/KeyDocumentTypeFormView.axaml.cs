using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.KeyDocumentType
{
    public partial class KeyDocumentTypeFormView : Window
    {
        public KeyDocumentTypeFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
        }

        ~KeyDocumentTypeFormView()
        {
            WindowManager.Remove(this);
        }
    }
}
