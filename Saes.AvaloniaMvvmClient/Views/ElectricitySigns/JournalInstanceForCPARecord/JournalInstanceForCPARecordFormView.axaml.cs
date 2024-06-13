using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.ElectricitySigns.JournalInstanceForCPARecord
{
    public partial class JournalInstanceForCPARecordFormView : Window
    {
        public JournalInstanceForCPARecordFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);

            
        }

        ~JournalInstanceForCPARecordFormView()
        {
            WindowManager.Remove(this);
        }
    }
}
