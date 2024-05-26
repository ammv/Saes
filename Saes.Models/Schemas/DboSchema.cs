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
    public static class dboSchema
    {
        private static readonly string _schemaName = "dbo";

        private static IQueryable<byte[]> udfHashSalt_Query(SaesContext ctx, string value, byte[] salt)
        {
            SqlParameterBuilder sqlParameterBuilder = new SqlParameterBuilder();

            var sqlParameterBuilderResult = sqlParameterBuilder
                .AddInput(value)
                .AddInput(salt)
                .Build();

            var sql = $"SELECT [{_schemaName}].[{SchemaBase.GetFunctionName()}]({sqlParameterBuilderResult.SqlParametersString}) as [Value]";

            var query = ctx.Database.SqlQueryRaw<byte[]>(sql, sqlParameterBuilderResult.SqlParametersObject);

            return query;
        }

        public static byte[] udfHashSalt(this SaesContext ctx, string value, byte[] salt)
        {
            var query = udfHashSalt_Query(ctx, value, salt);
            var result = query.Single();

            return result;
        }

        public static async Task<byte[]> udfHashSaltAsync(this SaesContext ctx, string value, byte[] salt)
        {
            var query = udfHashSalt_Query(ctx, value, salt);
            var result = await query.SingleAsync();

            return result;
        }
    }
}
