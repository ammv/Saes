using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views.HumanResources.Employee
{
    public partial class EmployeeFormView : Window
    {
        public EmployeeFormView()
        {
            InitializeComponent();
            WindowManager.Add(this);
            Closed += (s,e) => WindowManager.Remove(this);
        }
    }
}
