using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models.Schemas
{
    public static class SqlQueryHelper
    {
        public static string FunctionTemplate => "SELECT [{0}].[{1}]({2}) As [Value]";
        public static string ProcedureTemplate => "EXEC [{0}].[{1}] {2}";
        public static string GetFunctionName([CallerMemberName] string name = null)
        {
            return name.Replace("_Query", string.Empty);
        }

        public static string SqlProc(string schemaName,  string functionName, string sqlParams)
        {
            return string.Format(ProcedureTemplate, schemaName, functionName, sqlParams);
        }

        public static string SqlFunc(string schemaName, string functionName, string sqlParams)
        {
            return string.Format(FunctionTemplate, schemaName, functionName, sqlParams);
        }

        //protected string _schemaName => this.GetType().Name.Replace("Schema", "");
        //protected string _schemaPrefix => $"[{_schemaName}]";

        //protected string _functionPrefix([CallerMemberName] string name = null)
        //{
        //    return $"[{name}]";
        //}
    }
}
