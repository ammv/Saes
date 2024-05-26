using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models.Extensions
{

    public class SqlParameterBuilderResult
    {
        public SqlParameterBuilderResult(string sqlParametersString, IReadOnlyList<SqlParameter> sqlParameters)
        {
            SqlParametersString = sqlParametersString;
            SqlParameters = sqlParameters;
        }

        public string SqlParametersString { get; }
        public IReadOnlyList<SqlParameter> SqlParameters { get; }
        public IReadOnlyList<SqlParameter> SqlParametersOutput => SqlParameters.Where(x => x.Direction == ParameterDirection.Output).ToList();
        public object[] SqlParametersObject => SqlParameters.Select(x => x.Value).ToArray();
    }

    public class SqlParameterBuilder
    {
        private List<string> _paramNames = new List<string>();
        private string _getNewParam => $"@p{_paramNames.Count}";
        private List<SqlParameter> _params = new List<SqlParameter>();

        public void Clear()
        {
            _paramNames.Clear();
            _params.Clear();
        }

        private string AddParamName(string paramName)
        {
            _paramNames.Add(paramName);
            return paramName;
        }

        private void EnsureParamNameIsValid(string paramName)
        {
            if (_paramNames.Contains(paramName))
            {
                throw new ArgumentException($"The parameter {paramName} already exists", nameof(paramName));
            }

            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentException("Empty parameter name", nameof(paramName));
            }

            if (!paramName.StartsWith("@"))
            {
                throw new ArgumentException($"Invalid name of parameter: {paramName}", nameof(paramName));
            }
        }

        public SqlParameterBuilder AddInput(object value)
        {
            
            _params.Add(
                new SqlParameter
                {
                    ParameterName = AddParamName(_getNewParam),
                    Value = value,
                    Direction = ParameterDirection.Input
                });
            return this;
        }
        public SqlParameterBuilder AddOutput(string paramName)
        {
            EnsureParamNameIsValid(paramName);
            _params.Add(new SqlParameter
            {
                ParameterName = AddParamName(paramName),
                Direction = ParameterDirection.Output
            });
            return this;
        }
        public SqlParameterBuilder AddOutput(SqlDbType type)
        {
            _params.Add(new SqlParameter
            {
                ParameterName = AddParamName(_getNewParam),
                SqlDbType = type,
                Direction = ParameterDirection.Output
            });
            return this;
        }
        public SqlParameterBuilder AddOutput(string paramName, SqlDbType type)
        {
            EnsureParamNameIsValid(paramName);
            _params.Add(new SqlParameter
            {
                ParameterName = AddParamName(paramName),
                SqlDbType = type,
                Direction = ParameterDirection.Output
            });
            return this;
        }
        public SqlParameterBuilderResult Build()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _params.Count; i++)
            {
                var param = _params[i];

                if (param.Direction == ParameterDirection.Input)
                {
                    sb.Append(param.ParameterName);
                }

                else if (param.Direction == ParameterDirection.Output)
                {
                    sb.Append($"{param.ParameterName} OUTPUT");
                }

                if (i != _params.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            SqlParameterBuilderResult result = new SqlParameterBuilderResult(sb.ToString(), _params);

            return result;
        }

    }
}
