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

        private string _title;

		public string Title
		{
			get { return _title; }
			 set => this.RaiseAndSetIfChanged(ref _title, value);
		}

		private ViewModelCloseableBase _contentViewModel;

		public ViewModelCloseableBase Content
		{
			get { return _contentViewModel; }
            set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        public TabStripItemViewModel(string title, ViewModelCloseableBase content)
        {
			Title = title;
            Content = content;
        }

		public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public TabStripItemViewModel()
        {
            CloseCommand = ReactiveCommand.Create(OnCloseCommand);
        }

        private void OnCloseCommand()
        {
            bool closed = Content.Close();
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
