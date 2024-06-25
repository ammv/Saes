using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Saes.AvaloniaMvvmClient.Core.AttachedProperties;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Views;
using Saes.AvaloniaMvvmClient.Views.Authentication;
using Avalonia.LogicalTree;

namespace Saes.AvaloniaMvvmClient
{
    public class ViewLocator : IDataTemplate
    {
        private static List<Type> _exceptTypes = new List<Type> { typeof(MainWindow), typeof(AuthenticationMainView), typeof(FirstFactorAuthenticationView), typeof(SecondFactorAuthenticationView) }; 
        public bool SupportsRecycling => false;

        public Control Build(object data)
        {
            var name = data.GetType().FullName.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                var control = (Control)Activator.CreateInstance(type);

                if(!_exceptTypes.Contains(type))
                {
                    if (control is Window window)
                    {
                        window.Loaded += Control_ConfigureRights_Loaded;
                    }
                    else if (control is UserControl userControl)
                    {
                        userControl.Loaded += Control_ConfigureRights_Loaded;
                    }
                }
                
                return control;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        private async void Control_ConfigureRights_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            
            Control controlSender = (Control)sender;
            await VisualTreeRightPropertyWalking(controlSender, App.ServiceProvider.GetService<IUserService>().GetRights());
        }

        private async Task VisualTreeRightPropertyWalking(Visual controlSender, IReadOnlyCollection<string> rights)
        {
            foreach (var child in controlSender.GetVisualChildren())
            {
                if (child is Control control)
                {
                    await configureRight(rights, control);

                    var contextMenu = control.ContextMenu;
                    if (contextMenu != null)
                    {
                        foreach(Control item in contextMenu.Items)
                        {
                            if(item != null)
                            {
                                await configureRight(rights, item);
                            }
                        }
                    }
                }

                // Рекурсивный вызов для проверки всех дочерних элементов
                if (child is Visual visualChild)
                {
                    await VisualTreeRightPropertyWalking(visualChild, rights);
                }
            }
        }

        private static async Task configureRight(IReadOnlyCollection<string> rights, Control control)
        {
            var propValue = control.GetValue(RightBehav.RightCodeProperty);
            if (!string.IsNullOrEmpty(propValue))
            {
                if (!rights.Contains(propValue))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        control.IsVisible = false;
                    });
                }

            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
