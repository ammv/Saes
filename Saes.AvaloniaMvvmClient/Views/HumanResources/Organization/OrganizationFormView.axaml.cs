using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Organization;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.Views.HumanResources.Organization
{
    public partial class OrganizationFormView : Window
    {
        public OrganizationFormView()
        {
            InitializeComponent();

            WindowManager.Add(this);
            Closed += (s,e) => WindowManager.Remove(this);

        }
    }
}
