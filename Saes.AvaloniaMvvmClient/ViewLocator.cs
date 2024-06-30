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
using Saes.AvaloniaMvvmClient.ViewModels.Other;

namespace Saes.AvaloniaMvvmClient
{
    public class ViewLocator : IDataTemplate
    {
        private static List<Type> _exceptTypes = new List<Type> { typeof(MainWindow), typeof(AuthenticationMainView), typeof(FirstFactorAuthenticationView), typeof(SecondFactorAuthenticationView) };
        private Dictionary<ViewModelTabBase, Control> _viewModelViewDict = null;
        public bool SupportsRecycling => false;

        private void ConfigureViewDictironary()
        {
            if(_viewModelViewDict == null)
            {
                _viewModelViewDict = new();
                var vm = App.ServiceProvider.GetService<TabStripViewModel>();
                vm.TabRemoved += TabStripViewModel_TabRemoved;
            }
        }

        private void TabStripViewModel_TabRemoved(object sender, TabStripItemViewModel e)
        {
            var tab = _viewModelViewDict[e.Content];
            tab.DataContext = null;
            _viewModelViewDict.Remove(e.Content);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public Control Build(object data)
        {
            ConfigureViewDictironary();

            if (data is ViewModelTabBase vm1 && _viewModelViewDict.TryGetValue(vm1, out var c))
            {
                return c;
            }

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
                        // Saving tab
                        if(data is ViewModelTabBase vm2)
                        {
                            _viewModelViewDict[vm2] = control;
                        }
                        
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
