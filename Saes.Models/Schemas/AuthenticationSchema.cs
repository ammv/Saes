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
    public static class AuthenticationSchema
    {
        private static readonly string _schemaName = "Authentication";

        private static IQueryable<int> udfGetExistingUserIDByLogin_Query(SaesContext ctx, string userLogin)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(userLogin)
                .Build();

            var sql = $"SELECT [{_schemaName}].[{SqlQueryHelper.GetFunctionName()}]({sqlParameterBuilderResult.SqlParametersString}) as [Value]";

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

            var sql = string.Format(SqlQueryHelper.FunctionTemplate, _schemaName, SqlQueryHelper.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);
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

        private static SqlQueryData uspAddUser_Query(string login, string password, int userRoleId)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(login)
                .AddInput(password)
                .AddInput(userRoleId)
                .Build();

            var sql = SqlQueryHelper.SqlProc(_schemaName, SqlQueryHelper.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);

            return new SqlQueryData(sql, sqlParameterBuilderResult.SqlParameters.ToArray());
        }

        /// <summary>
        /// Executing procedure uspAddUser in database and return SessionKey
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <param name="expiredAt"></param>
        /// <returns>Session key</returns>
        public static int uspAddUser(this SaesContext ctx, string login, string password, int userRoleId)
        {
            var sqlQueryData = uspAddUser_Query(login, password, userRoleId);
            return ctx.Database.ExecuteSqlRaw(sqlQueryData.Sql, sqlQueryData.SqlParameters);
        }

        public static async Task<int> uspAddUserAsync(this SaesContext ctx, string login, string password, int userRoleId)
        {
            var sqlQueryData = uspAddUser_Query(login, password, userRoleId);
            return await ctx.Database.ExecuteSqlRawAsync(sqlQueryData.Sql, sqlQueryData.SqlParameters);
        }

        private static SqlQueryData uspUpdatePasswordUser_Query(string login, string password)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(login)
                .AddInput(password)
                .Build();

            var sql = SqlQueryHelper.SqlProc(_schemaName, SqlQueryHelper.GetFunctionName(), sqlParameterBuilderResult.SqlParametersString);

            return new SqlQueryData(sql, sqlParameterBuilderResult.SqlParameters.ToArray());
        }

        /// <summary>
        /// Executing procedure uspUpdatePasswordUser in database and return SessionKey
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userId"></param>
        /// <param name="expiredAt"></param>
        /// <returns>Session key</returns>
        public static int uspUpdatePasswordUser(this SaesContext ctx, string login, string password)
        {
            var sqlQueryData = uspUpdatePasswordUser_Query(login, password);
            return ctx.Database.ExecuteSqlRaw(sqlQueryData.Sql, sqlQueryData.SqlParameters);
        }

        public static async Task<int> uspUpdatePasswordUserAsync(this SaesContext ctx, string login, string password)
        {
            var sqlQueryData = uspUpdatePasswordUser_Query(login, password);
            return await ctx.Database.ExecuteSqlRawAsync(sqlQueryData.Sql, sqlQueryData.SqlParameters);
        }
    }
}
