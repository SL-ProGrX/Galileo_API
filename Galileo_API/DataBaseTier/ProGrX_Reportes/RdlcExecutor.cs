using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Galileo.DataBaseTier.ProGrX_Reportes;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Ejecuta consultas o procedimientos almacenados para obtener los datasets de un reporte RDLC.
    /// Refactorizada para reducir la complejidad cognitiva y mejorar mantenibilidad.
    /// </summary>
    public sealed class RdlcExecutor : IRdlcExecutor
    {
        // Timeout común para todas las expresiones regulares de esta clase
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        public bool TryExecDataSet(
            SqlConnection connection,
            RdlcDataSetMeta ds,
            IDictionary<string, string> ctx,
            JObject? jsonParams,
            bool allowFiltrosReplacement,
            out IEnumerable<object> rows,
            out string? error)
        {
            rows = Enumerable.Empty<object>();
            error = null;

            try
            {
                var paramPairs = BuildParameterPairs(ds, ctx, jsonParams);
                var sqlText    = PrepareSql(ds, jsonParams, allowFiltrosReplacement);
                var dp         = BuildDynamicParameters(paramPairs);

                return ExecuteQuery(connection, ds, sqlText, dp, out rows, out error);
            }
            catch (Exception ex)
            {
                error = $"[{ds.DataSetName}] {ex.GetType().Name}: {ex.Message}";
                rows  = Enumerable.Empty<object>();
                return false;
            }
        }

        // =====================================================
        // =============== MÉTODOS PRIVADOS ====================
        // =====================================================

        private static List<(string Name, object? Value)> BuildParameterPairs(
            RdlcDataSetMeta ds,
            IDictionary<string, string> ctx,
            JObject? jsonParams)
        {
            var paramPairs = new List<(string Name, object? Value)>();

            // Parámetros definidos en el RDLC
            foreach (var (qpName, expr) in ds.QueryParams)
            {
                var raw = EvalExpr(expr, ctx);
                paramPairs.Add((qpName!, CoerceEmptyToNull(raw)));
            }

            // Parámetros adicionales del JSON
            var existing = new HashSet<string>(
                paramPairs.Select(p => NormalizeParamName(p.Name)),
                StringComparer.OrdinalIgnoreCase);

            if (jsonParams == null)
                return paramPairs;

            foreach (var prop in jsonParams.Properties())
            {
                var name = prop.Name;
                if (string.Equals(name, "filtros", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (existing.Contains(name))
                    continue;

                var val = prop.Value?.ToString();
                paramPairs.Add((name, string.IsNullOrWhiteSpace(val) ? null : val));
                existing.Add(name);
            }

            return paramPairs;
        }

        private static string PrepareSql(
            RdlcDataSetMeta ds,
            JObject? jsonParams,
            bool allowFiltrosReplacement)
        {
            var sqlText = ds.CommandText?.Trim() ?? string.Empty;
            var isStoredProc = string.Equals(ds.CommandType, "StoredProcedure", StringComparison.OrdinalIgnoreCase)
                               || LooksLikeSpName(sqlText);

            if (!isStoredProc && allowFiltrosReplacement &&
                jsonParams != null &&
                jsonParams.TryGetValue("filtros", out var filtrosToken))
            {
                var f = filtrosToken?.ToString();
                if (!string.IsNullOrWhiteSpace(f))
                    sqlText = sqlText.Replace("@filtros", f, StringComparison.Ordinal);
            }

            return sqlText;
        }

        private static DynamicParameters BuildDynamicParameters(IEnumerable<(string Name, object? Value)> paramPairs)
        {
            var dp = new DynamicParameters();
            foreach (var (name, val) in paramPairs)
                dp.Add("@" + NormalizeParamName(name), val);
            return dp;
        }

        private static bool ExecuteQuery(
            SqlConnection connection,
            RdlcDataSetMeta ds,
            string sqlText,
            DynamicParameters dp,
            out IEnumerable<object> rows,
            out string? error)
        {
            error = null;
            rows  = Enumerable.Empty<object>();

            var isStoredProc = string.Equals(ds.CommandType, "StoredProcedure", StringComparison.OrdinalIgnoreCase)
                               || LooksLikeSpName(sqlText);
            var isExecBatch = sqlText.StartsWith("exec ", StringComparison.OrdinalIgnoreCase);

            if (isStoredProc && !isExecBatch)
                return ExecuteStoredProcedure(connection, sqlText, dp, out rows, out error);

            rows = connection.Query(sqlText, dp, commandType: CommandType.Text).ToList();
            return true;
        }

        private static bool ExecuteStoredProcedure(
            SqlConnection connection,
            string sqlText,
            DynamicParameters dp,
            out IEnumerable<object> rows,
            out string? error)
        {
            error = null;
            rows  = Enumerable.Empty<object>();

            var result = connection.Query(sqlText, dp, commandType: CommandType.StoredProcedure).ToList();
            if (result.Count > 0)
            {
                rows = result;
                return true;
            }

            using var reader = connection.ExecuteReader(sqlText, dp, commandType: CommandType.StoredProcedure);
            var schema = reader.GetSchemaTable();
            if (schema == null || schema.Rows.Count == 0)
                return true;

            var empty = new ExpandoObject() as IDictionary<string, object?>;
            foreach (DataRow col in schema.Rows)
            {
                var colName = col["ColumnName"]?.ToString();
                if (!string.IsNullOrEmpty(colName) && !empty.ContainsKey(colName))
                    empty[colName] = null;
            }

            rows = new List<object> { empty! };
            return true;
        }

        // =====================================================
        // =============== MÉTODOS AUXILIARES ==================
        // =====================================================

        private static object EvalExpr(string? expr, IDictionary<string, string> ctx)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return DBNull.Value;

            var e = expr.Trim();

            // Antes: Regex.Match(e, @"^=Parameters!(?<p>\w+)\.Value$", RegexOptions.IgnoreCase);
            var m = Regex.Match(
                e,
                @"^=Parameters!(?<p>\w+)\.Value$",
                RegexOptions.IgnoreCase,
                RegexTimeout);

            if (m.Success)
            {
                var pname = m.Groups["p"].Value;
                return ctx.TryGetValue(pname, out var v)
                    ? (object)(v ?? string.Empty)
                    : DBNull.Value;
            }

            if (decimal.TryParse(e, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;

            return e.Trim('"', '\'');
        }

        private static string NormalizeParamName(string name)
        {
            return name.Trim().TrimStart('@');
        }

        private static bool LooksLikeSpName(string? commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return false;

            var q = commandText.Trim();
            if (q.IndexOf(' ') >= 0) return false;

            return !(
                q.StartsWith("select", StringComparison.OrdinalIgnoreCase) ||
                q.StartsWith("with",   StringComparison.OrdinalIgnoreCase) ||
                q.StartsWith("insert", StringComparison.OrdinalIgnoreCase) ||
                q.StartsWith("update", StringComparison.OrdinalIgnoreCase) ||
                q.StartsWith("delete", StringComparison.OrdinalIgnoreCase));
        }

        private static object? CoerceEmptyToNull(object? val)
        {
            if (val is null)
                return null;

            if (val is JValue jv)
            {
                if (jv.Type == JTokenType.Null)
                    return null;
                val = jv.Value;
                if (val is null)
                    return null;
            }

            if (val is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    return null;
                if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase))
                    return null;
                return s;
            }

            return val;
        }
    }
}