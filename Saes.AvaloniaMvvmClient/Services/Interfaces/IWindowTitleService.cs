using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Interfaces
{
    public interface IWindowTitleService
    {
        public string Title { get; }
        public string TitleFormat { get; set; }
        public IEnumerable<string> Keys { get;  }
        public void Remove(string key);
        public void AddOrUpdate(string key, string value);
    }
}
