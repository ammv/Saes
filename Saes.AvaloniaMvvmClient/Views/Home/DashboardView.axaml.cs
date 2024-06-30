using Avalonia.Controls;
using System.Diagnostics;

namespace Saes.AvaloniaMvvmClient.Views.Home
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        ~DashboardView()
        {
            Debug.WriteLine("DashboardView destructed");
        }
    }
}
