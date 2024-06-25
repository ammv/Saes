using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class WindowTitleService : ReactiveObject, IWindowTitleService
    {
        private Dictionary<string, string> _titleProperties = new Dictionary<string, string>();
        private string _titleFormat;
        private string _title;

        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string TitleFormat
        { 
            get => _titleFormat;
            set
            {
                this.RaiseAndSetIfChanged(ref _titleFormat, value);
                UpdateTitle();
            }
        }

        public IEnumerable<string> Keys => _titleProperties.Keys;

        public void AddOrUpdate(string key, string value)
        {
            _titleProperties[key] = value;
            UpdateTitle();
        }

        public void Remove(string key)
        {
            _titleProperties.Remove(key);
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if(string.IsNullOrEmpty(_titleFormat))
            {
                return;
            }

            string newTitle = _titleFormat;

            foreach(var kv in _titleProperties)
            {
                newTitle = newTitle.Replace($"{{{kv.Key}}}", kv.Value);
            }
            Title = newTitle;
        }
    }
}
