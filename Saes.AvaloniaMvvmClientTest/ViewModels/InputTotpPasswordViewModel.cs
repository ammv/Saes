using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace Saes.AvaloniaMvvmClientTest.ViewModels
{
    public class InputTotpPasswordViewModel: ViewModelBase
    {
		private string _totpPassword;

		public string TotpPassword
		{
			get { return _totpPassword; }
            set => this.RaiseAndSetIfChanged(ref _totpPassword, value);
        }

        public ReactiveCommand<Unit, string> SuccessCommand { get; }

        private string SuccessCommandOnExecute()
        {
            return _totpPassword;
        }

        public InputTotpPasswordViewModel()
        {
            var isValidObservable = this.WhenAnyValue(x => x.TotpPassword, x => !string.IsNullOrEmpty(x) && x.Replace(" ", "").Trim().Length == 6);
            SuccessCommand = ReactiveCommand.Create(SuccessCommandOnExecute);

        }
    }
}
