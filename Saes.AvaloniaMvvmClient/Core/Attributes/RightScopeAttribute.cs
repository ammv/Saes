using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core.Attributes
{
    public class RightScopeAttribute : Attribute
    {
        public string[] RightScope { get; }
        public RightScopeAttribute(params string[] rightsScope) 
        {
            RightScope = rightsScope;
        }
    }
}
