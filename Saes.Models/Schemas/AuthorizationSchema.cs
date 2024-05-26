using Microsoft.EntityFrameworkCore;
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
        private static IQueryable<string> uspCreateSession_Query(SaesContext ctx, int userId, DateTime expiredAt)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(userId)
                .AddInput(expiredAt)
                .AddOutput("@sessionKey", System.Data.SqlDbType.NVarChar)
                .Build();

            var sql = string.Format(SchemaBase.ProcedureTemplate, _schemaName, SchemaBase.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);

            var query = ctx.Database.SqlQueryRaw<string>(sql, sqlParameterBuilderResult.SqlParametersObject);

            return query;
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
            var query = uspCreateSession_Query(ctx, userId, expiredAt);
            var result = query.Single();


            return result;
        }

        public static async Task<string> uspCreateSessionAsync(this SaesContext ctx, int userId, DateTime expiredAt)
        {
            var query = uspCreateSession_Query(ctx, userId, expiredAt);
            var result = await query.SingleAsync();

            return result;
        }


        private static IQueryable<bool> udfVerifyUser_Query(SaesContext ctx, string userLogin, string userPassword)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(userLogin)
                .AddInput(userPassword)
                .Build();

            var sql = string.Format(SchemaBase.FunctionTemplate, _schemaName, SchemaBase.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);
            //var sql = $"SELECT [{_schemaName}].[{SchemaBase.GetFunctionName()}]({sqlParameterBuilderResult.SqlParametersString}) as [Value]";

            var query = ctx.Database.SqlQueryRaw<bool>(sql, sqlParameterBuilderResult.SqlParameters);

            return query;
        }

        public static bool udfVerifyUser(this SaesContext ctx, string userLogin, string userPassword)
        {
            var query = udfVerifyUser_Query(ctx, userLogin, userPassword);
            var result = query.Single();

            
            return result;
        }

        public static async Task<bool> udfVerifyUserAsync(this SaesContext ctx, string userLogin, string userPassword)
        {
            var query = udfVerifyUser_Query(ctx, userLogin, userPassword);
            var result = await query.SingleAsync();

            return result;
        }
    }
}
