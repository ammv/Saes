using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.JournalInstanceForCIHRecord
{
    public partial class JournalInstanceForCIHRecordFormView : Window
    {
        public JournalInstanceForCIHRecordFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s, e) => WindowManager.Remove(this);
        }

    }
}
