using Avalonia.Controls;
using System.Diagnostics;

namespace Saes.AvaloniaMvvmClient.Views.Audit.LogAuthentication
{
    public partial class LogAuthenticationListView : UserControl
    {
        public LogAuthenticationListView()
        {
            InitializeComponent();

        }

        ~LogAuthenticationListView()
        {
            Debug.WriteLine("LogAuthenticationListView destructed");
        }
    }
}
