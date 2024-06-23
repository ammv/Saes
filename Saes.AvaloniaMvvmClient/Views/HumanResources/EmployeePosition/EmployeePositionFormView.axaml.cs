using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.HumanResources.EmployeePosition
{
    public partial class EmployeePositionFormView : Window
    {
        public EmployeePositionFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s, e) => WindowManager.Remove(this);
        }
    }
}
