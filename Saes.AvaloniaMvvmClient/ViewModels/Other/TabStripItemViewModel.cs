using ReactiveUI;
using System;
using System.Collections.Generic;
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

		public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public TabStripItemViewModel()
        {
            CloseCommand = ReactiveCommand.Create(OnCloseCommand);
        }

        private async void OnCloseCommand()
        {

            bool closed = await Content.CloseAsync();
            if(closed)
            {
                OnClosed();
            }
        }

        private void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }


    }
}
