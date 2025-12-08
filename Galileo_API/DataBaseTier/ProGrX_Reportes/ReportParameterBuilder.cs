using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json.Linq;
using Galileo.Models;

namespace Galileo.DataBaseTier
{
    internal static class ParamKeys
    {
        public const string UrlLogo   = "urlLogo";
        public const string Empresa   = "Empresa";
        public const string ConString = "conString";
        public const string Filtros   = "filtros";
    }
    public sealed class ReportParameterBuilder : IReportParameterBuilder
    {
        public (List<ReportParameter> reportParams, Dictionary<string, string> ctx, JObject? jsonParams)
            Build(FrmReporteGlobal data, SqlConnection connection, string connString)
        {
            var reportParams = new List<ReportParameter>();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            JObject? jParams = null;

            if (string.IsNullOrWhiteSpace(data.parametros))
                return (reportParams, dict, jParams);

            jParams = JObject.Parse(data.parametros);

            foreach (var prop in jParams.Properties())
            {
                if (prop.Name.Equals(ParamKeys.UrlLogo, StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals(ParamKeys.Empresa, StringComparison.OrdinalIgnoreCase))
                    continue;

                var val = prop.Value?.ToString() ?? string.Empty;
                reportParams.Add(new ReportParameter(prop.Name, val));
                dict[prop.Name] = val;
            }

            if (data.parametros.Contains( ParamKeys.ConString, StringComparison.OrdinalIgnoreCase))
            {
                reportParams.Add(new ReportParameter(ParamKeys.ConString, connString));
                dict[ParamKeys.ConString] = connString;
            }

            if (data.parametros.Contains(ParamKeys.UrlLogo, StringComparison.OrdinalIgnoreCase))
            {
                var logo = connection.QueryFirstOrDefault<string>("SELECT LOGO_WEB_SITE FROM SIF_EMPRESA") ?? string.Empty;
                reportParams.Add(new ReportParameter(ParamKeys.UrlLogo, logo));
                dict[ParamKeys.UrlLogo] = logo;
            }

            if (data.parametros.Contains(ParamKeys.Empresa, StringComparison.OrdinalIgnoreCase))
            {
                var nombreEmpresa = connection.QueryFirstOrDefault<string>("SELECT Nombre FROM SIF_EMPRESA") ?? string.Empty;
                reportParams.Add(new ReportParameter(ParamKeys.Empresa, nombreEmpresa));
                dict[ParamKeys.Empresa] = nombreEmpresa;
            }

            return (reportParams, dict, jParams);
        }
    }
}
