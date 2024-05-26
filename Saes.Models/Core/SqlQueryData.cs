using Microsoft.Data.SqlClient;
using Saes.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models.Core
{
    public class SqlQueryData
    {
        public SqlQueryData(string sql, SqlParameter[] sqlParameters)
        {
            Sql = sql;
            SqlParameters = sqlParameters;
        }

        public string Sql { get; set; }
        public SqlParameter[] SqlParameters { get; set; }    
    }
}
