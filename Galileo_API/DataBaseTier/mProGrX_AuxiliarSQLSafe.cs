using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;
using Dapper;

namespace Galileo.DataBaseTier
{
    internal static partial class SqlSafe
    {
        // Identificadores SQL permitidos (tabla/columna): letras, números, _
        private static readonly Regex IdentRegex =
            MyRegex();

        public static string Ident(string ident)
        {
            if (string.IsNullOrWhiteSpace(ident) || !IdentRegex.IsMatch(ident))
                throw new SecurityException("Identificador SQL inválido.");

            return $"[{ident}]";
        }

        /// <summary>
        /// Convierte un whereClause a SQL parametrizado.
        /// Soporta:
        ///   Col = 123
        ///   Col = 'ABC'
        ///   Col = 'A''BC'
        ///   ... AND ...
        /// </summary>
        public static (string whereSql, DynamicParameters dp) Where(string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                throw new SecurityException("WHERE vacío.");

            if (ContainsUnsafeTokens(whereClause))
                throw new SecurityException("WHERE contiene tokens no permitidos.");

            var parts = MyRegex1().Split(whereClause)
                             .Select(p => p.Trim())
                             .Where(p => p.Length > 0)
                             .ToList();

            var dp = new DynamicParameters();
            var clauses = new List<string>();
            int i = 0;

            foreach (var part in parts)
            {
                var (col, rawVal) = ParseWherePart(part);
                object val = ParseValue(rawVal);

                var p = $"@w{i++}";
                dp.Add(p, val);
                clauses.Add($"{Ident(col)} = {p}");
            }

            return (string.Join(" AND ", clauses), dp);
        }

        private static bool ContainsUnsafeTokens(string whereClause)
        {
            return whereClause.Contains(";") ||
                   whereClause.Contains("--") ||
                   whereClause.Contains("/*") ||
                   whereClause.Contains("*/");
        }

        private static (string col, string rawVal) ParseWherePart(string part)
        {
            var m = MyRegex2().Match(part);

            if (!m.Success)
                throw new SecurityException("WHERE no permitido. Use: Col=123 o Col='ABC' (con AND).");

            var col = m.Groups["col"].Value;
            var rawVal = m.Groups["val"].Value.Trim();

            if (!IdentRegex.IsMatch(col))
                throw new SecurityException("Columna inválida en WHERE.");

            return (col, rawVal);
        }

        private static object ParseValue(string rawVal)
        {
            if (rawVal.StartsWith("'", StringComparison.Ordinal))
            {
                return rawVal[1..^1].Replace("''", "'");
            }
            else if (rawVal.Contains('.'))
            {
                if (!decimal.TryParse(rawVal, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    throw new SecurityException("Número inválido en WHERE.");
                return dec;
            }
            else
            {
                if (!long.TryParse(rawVal, out var lng))
                    throw new SecurityException("Número inválido en WHERE.");
                return lng;
            }
        }

        [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"\s+AND\s+", RegexOptions.IgnoreCase, "es-CR")]
        private static partial Regex MyRegex1();
        [GeneratedRegex(@"^(?<col>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<val>(\d+(\.\d+)?)|('([^']|(''))*'))$", RegexOptions.IgnoreCase, "es-CR")]
        private static partial Regex MyRegex2();
    }
}