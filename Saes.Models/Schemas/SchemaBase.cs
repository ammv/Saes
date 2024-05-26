using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models.Schemas
{
    public static class SchemaBase
    {
        public static string FunctionTemplate => "SELECT [{0}].[{1}]({2}) As [Value]";
        public static string ProcedureTemplate => "SELECT [{0}].[{1}]({2}) As [Value]";
        public static string GetFunctionName([CallerMemberName] string name = null)
        {
            return name.Replace("_Query", string.Empty);
        }

        //protected string _schemaName => this.GetType().Name.Replace("Schema", "");
        //protected string _schemaPrefix => $"[{_schemaName}]";

        //protected string _functionPrefix([CallerMemberName] string name = null)
        //{
        //    return $"[{name}]";
        //}
    }
}
