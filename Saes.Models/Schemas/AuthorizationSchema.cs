using Microsoft.EntityFrameworkCore;
using Saes.Models.Core;
using Saes.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models.Schemas
{
    public static class AuthorizationSchema
    {
        private static readonly string _schemaName = "Authorization";
//        CREATE PROCEDURE[Authorization].[uspCreateSession]
//        (
//    @UserID int,
//	@ExpiredAt datetime,
//    @SessionKey uniqueidentifier OUTPUT
//)
        private static SqlQueryData uspCreateSession_Query(int userId, DateTime expiredAt)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(userId)
                .AddInput(expiredAt)
                .AddOutput("@sessionKey", System.Data.SqlDbType.NVarChar, 128)
                .Build();

            var sql = SqlQueryHelper.SqlProc(_schemaName, SqlQueryHelper.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);

            return new SqlQueryData(sql, sqlParameterBuilderResult.SqlParameters.ToArray());
        }

        /// <summary>
        /// Executing procedure uspCreateSession in database and return SessionKey
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <param name="expiredAt"></param>
        /// <returns>Session key</returns>
        public static string uspCreateSession(this SaesContext ctx, int userId, DateTime expiredAt)
        {
            var sqlQueryData = uspCreateSession_Query(userId, expiredAt);
            var result = ctx.Database.ExecuteSqlRaw(sqlQueryData.Sql, sqlQueryData.SqlParameters);

            return (string)sqlQueryData.SqlParameters.Last().Value;
        }

        public static async Task<string> uspCreateSessionAsync(this SaesContext ctx, int userId, DateTime expiredAt)
        {
            var sqlQueryData = uspCreateSession_Query(userId, expiredAt);
            var result = await ctx.Database.ExecuteSqlRawAsync(sqlQueryData.Sql, sqlQueryData.SqlParameters);

            return (string)sqlQueryData.SqlParameters.Last().Value;
        }


        
    }
}
