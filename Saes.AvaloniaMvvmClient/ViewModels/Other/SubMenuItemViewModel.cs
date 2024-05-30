using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Other
{
    public class SubMenuItemViewModel: ViewModelBase
    {
        public event EventHandler SubMenuItemClicked;

        private string _title;

        public string Title
        {
            get { return _title; }
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private StreamGeometry _icon;

        public StreamGeometry Icon
        {
            get { return _icon; }
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }

        private Type _viewModelType;

        public Type ViewModelType
        {
            get { return _viewModelType; }
            set { _viewModelType = value; }
        }

        public SubMenuItemViewModel(string title, string iconKey, Type viewModelType)
        {
            Title = title;
            ViewModelType = viewModelType;

            if (iconKey != null)
            {
                App.Current.TryFindResource(iconKey, out var icon);

                Icon = icon as StreamGeometry;
            }

            ClickCommand = ReactiveCommand.Create<PointerReleasedEventArgs>(OnSubMenuItemClicked);
        }

        public ReactiveCommand<PointerReleasedEventArgs, Unit> ClickCommand { get; }

        private void OnSubMenuItemClicked(PointerReleasedEventArgs args)
        {
            if (args?.InitialPressMouseButton == MouseButton.Left)
            {
                SubMenuItemClicked?.Invoke(this, EventArgs.Empty);
            }
            
        }

    }
}
