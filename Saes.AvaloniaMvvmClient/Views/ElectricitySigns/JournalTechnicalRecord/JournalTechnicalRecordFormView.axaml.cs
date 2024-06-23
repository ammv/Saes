using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.JournalTechnicalRecord
{
    public partial class JournalTechnicalRecordFormView : Window
    {
        public JournalTechnicalRecordFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s, e) => WindowManager.Remove(this);
        }

    }
}
