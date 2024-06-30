using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.Other
{
    
    public class TabStripItemViewModel: ViewModelBase
    {
        public event EventHandler Closed;

		private ViewModelTabBase _contentViewModel;

		public ViewModelTabBase Content
		{
			get { return _contentViewModel; }
            set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        public TabStripItemViewModel(ViewModelTabBase content)
        {
            Content = content;
        }

		public ReactiveCommand<Avalonia.Input.PointerReleasedEventArgs, Unit> CloseCommand { get; }

        public TabStripItemViewModel()
        {
            CloseCommand = ReactiveCommand.CreateFromTask<Avalonia.Input.PointerReleasedEventArgs>(OnCloseCommand);
        }

        private async Task OnCloseCommand(Avalonia.Input.PointerReleasedEventArgs e)
        {
            if(e?.InitialPressMouseButton == Avalonia.Input.MouseButton.Middle)
            {
                OnClosed();
            }
            else if(e == null)
            {
                bool closed = await Content.CloseAsync();
                if (closed)
                {
                    OnClosed();
                }
            }
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
            Content = null;
        }

        ~TabStripItemViewModel()
        {
            Debug.WriteLine("TabStripItemViewModel destructed");   
        }
    }
}
