using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public class ExcelExporterConfig
    {
        private Dictionary<Type, Config> _configs = new Dictionary<Type, Config>();
        public sealed class Config
        {
            public Config(Func<object, ICollection<object>> dataExtractor, ICollection<object> headers, string suggestedFileNamePrefix)
            {
                DataExtractor = dataExtractor;
                Headers = headers;
                SuggestedFileNamePrefix = suggestedFileNamePrefix;
            }

            public Func<object, ICollection<object>> DataExtractor { get; set; }
            public ICollection<object> Headers { get; set; }
            public string SuggestedFileNamePrefix { get; set; }
        }

        public void AddConfig<T>(Func<object, ICollection<object>> dataExtractor, ICollection<object> headers, string suggestedFileNamePrefix)
        {
            Config config = new Config(dataExtractor, headers, suggestedFileNamePrefix);
            _configs[typeof(T)] = config;
        }

        public Config GetConfig<T>()
        {
            if(_configs.TryGetValue(typeof(T), out var config))
            {
                return config;
            }
            throw new Exception($"Config for {typeof(T).Name} not found");
        }

    }
}
