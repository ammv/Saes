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
    public static class AuthenticationSchema
    {
        private static readonly string _schemaName = "Authentication";

        private static IQueryable<int> udfGetExistingUserIDByLogin_Query(SaesContext ctx, string userLogin)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(userLogin)
                .Build();

            var sql = $"SELECT [{_schemaName}].[{SchemaBase.GetFunctionName()}]({sqlParameterBuilderResult.SqlParametersString}) as [Value]";

            var query = ctx.Database.SqlQueryRaw<int>(sql, sqlParameterBuilderResult.SqlParametersObject);

            return query;
        }

        public static int udfGetExistingUserIDByLogin(this SaesContext ctx, string userLogin)
        {
            var query = udfGetExistingUserIDByLogin_Query(ctx, userLogin);
            var result = query.Single();

            
            return result;
        }

        public static async Task<int> udfGetExistingUserIDByLoginAsync(this SaesContext ctx, string userLogin)
        {
            var query = udfGetExistingUserIDByLogin_Query(ctx, userLogin);
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

            var query = ctx.Database.SqlQueryRaw<bool>(sql, sqlParameterBuilderResult.SqlParametersObject);

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
