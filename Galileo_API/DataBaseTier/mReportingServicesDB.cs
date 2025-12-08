using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Galileo.DataBaseTier.ProGrX_Reportes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class MReportingServicesDBDependencies
    {
        public ILogger<MReportingServicesDB>? Logger { get; set; }
        public IRdlcPathResolver Path { get; set; }
        public IRdlcMetaReader Meta { get; set; }
        public IRdlcCodePatcher Patcher { get; set; }
        public IRdlcExecutor Exec { get; set; }
        public IReportParameterBuilder ParamBuilder { get; set; }
        public ISubreportCoordinator Subs { get; set; }

        public MReportingServicesDBDependencies() 
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Logger = loggerFactory.CreateLogger<MReportingServicesDB>();
            Path = new RdlcPathResolver();
            Meta = new RdlcMetaReader();
            Patcher = new RdlcCodePatcher();
            Exec = new RdlcExecutor();
            ParamBuilder = new ReportParameterBuilder();
            Subs = new SubreportCoordinator(Meta, Path, Exec);
        }

    }

    public class DataSetLoadRequest
    {
        public List<Galileo.DataBaseTier.ProGrX_Reportes.RdlcDataSetMeta>? mainDataSets { get; set; }
        public SqlConnection? connection { get; set; }
        public IDictionary<string, object>? paramDict { get; set; }
        public Newtonsoft.Json.Linq.JObject? jParams { get; set; }
        public FrmReporteGlobal? data { get; set; }
        public LocalReport? report { get; set; }
        public Dictionary<string, object>? jsonDataSets { get; set; }
        public List<string>? subErrors { get; set; }
    }

    /// <summary>
    /// Servicio para renderizar reportes RDLC (con subreportes y codeSection dinámico).
    /// Refactorizado en componentes para cumplir SonarQube.
    /// </summary>

    public class MReportingServicesDB
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MReportingServicesDB>? _logger;
        private readonly string _dirRdlc;

        private readonly IRdlcPathResolver _path;
        private readonly IRdlcMetaReader _meta;
        private readonly IRdlcCodePatcher _patcher;
        private readonly IRdlcExecutor _exec;
        private readonly IReportParameterBuilder _params;
        private readonly ISubreportCoordinator _subs;

        private readonly string DefaultLogoUrl;
        private readonly string DefaultEmpresa;

        private readonly MReportingServicesDBDependencies deps = new MReportingServicesDBDependencies();


        public MReportingServicesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = deps.Logger ;
            _path = deps.Path ;
            _meta = deps.Meta ;
            _patcher = deps.Patcher ;
            _exec = deps.Exec ;
            _params = deps.ParamBuilder ;
            _subs = deps.Subs ;

            _dirRdlc = _config.GetSection("AppSettings")["RutaRDLC"] ?? string.Empty;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DefaultLogoUrl = _config.GetSection("ReporteSrv")["DefaultLogoUrl"] ?? string.Empty;
            DefaultEmpresa = _config.GetSection("ReporteSrv")["DefaultEmpresa"] ?? string.Empty;
        }

        // ================= API PRINCIPAL (V2) =================
        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
        {
            if (data is null)
                return ReportRenderer.Error("Datos del reporte no proporcionados.", 400);

            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(data.codEmpresa);

            try
            {
                using var connection = new SqlConnection(connString);
                connection.Open();

                var report = new LocalReport { EnableExternalImages = true };
                var basePath = _path.GetBasePath(data.codEmpresa, data.folder, _dirRdlc);

                if (string.IsNullOrWhiteSpace(data.nombreReporte))
                    return ReportRenderer.Error("El nombre del reporte no puede ser nulo o vacío.", 400);

                var mainPath = _path.ResolveReportPath(basePath, data.nombreReporte);
                if (mainPath == null)
                    return ReportRenderer.Error($"No se encontró el reporte principal: {System.IO.Path.Combine(basePath, data.nombreReporte)}");

                using var patched = _patcher.PatchReportCode(mainPath, data.codeSection);
                report.LoadReportDefinition(patched);

                var (mainDataSets, subreportNames) = _meta.ReadRdlcMeta(mainPath);
                var subMeta = _subs.LoadSubreports(report, basePath, subreportNames);
                var autoAliases = _subs.BuildAutoAliasMap(mainPath, basePath);

                var (reportParams, paramDict, jParams) = _params.Build(data, connection, connString);
                if (reportParams.Count > 0)
                    report.SetParameters(reportParams);

                var jsonDataSets = new Dictionary<string, object>();
                var subErrors = new List<string>();

                // Ensure paramDict is Dictionary<string, object>
                var paramDictObj = paramDict is IDictionary<string, object> dictObj
                    ? dictObj
                    : paramDict.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

                var safeJParams = jParams ?? new Newtonsoft.Json.Linq.JObject();

                DataSetLoadRequest request = new DataSetLoadRequest
                {
                    mainDataSets = mainDataSets,
                    connection = connection,
                    paramDict = paramDictObj,
                    jParams = safeJParams,
                    data = data,
                    report = report,
                    jsonDataSets = jsonDataSets,
                    subErrors = subErrors
                };

                LoadMainDataSets(request);

                // Subreport processing
                _subs.ConfigureSubreportProcessing(report, subMeta, connection, autoAliases, paramDict, subErrors, _patcher.ParseFxConstants(data.codeSection));

                return data.cod_reporte == "P"
                    ? ReportRenderer.AsPdf(report, data.nombreReporte)
                    : ReportRenderer.AsJson(jsonDataSets, subErrors);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generando reporte RDLC");
                return ReportRenderer.Error(ex.Message);
            }
        }

        private void LoadMainDataSets(DataSetLoadRequest request)
        {
            if (request.mainDataSets == null || request.paramDict == null || request.connection == null || request.subErrors == null || request.jsonDataSets == null)
                return;

            foreach (var ds in request.mainDataSets)
            {
                if (ds == null || string.IsNullOrWhiteSpace(ds.CommandText))
                    continue;

                var paramDictStr = request.paramDict?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty)
                                   ?? new Dictionary<string, string>();

                if (!_exec.TryExecDataSet(request.connection, ds, paramDictStr, request.jParams, true, out var rows, out var err))
                {
                    if (!string.IsNullOrWhiteSpace(err))
                        request.subErrors?.Add($"[MAIN DS '{ds.DataSetName}'] {err}");
                    continue;
                }

                if (request.data != null && request.data.cod_reporte == "P")
                    request.report?.DataSources.Add(new ReportDataSource(ds.DataSetName, rows));
                else if (request.jsonDataSets != null)
                    request.jsonDataSets[ds.DataSetName] = rows;
            }
        }

        // ================= INFO EMPRESA =================
        public ErrorDto<object> ReportesInfo(int codEmpresa)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var resp = new ErrorDto<object> { Code = 0, Description = "OK", Result = new { LOGO_WEB_SITE = string.Empty, Nombre = string.Empty } };

            try
            {
                using var connection = new SqlConnection(connString);
                resp.Result = connection.QueryFirstOrDefault<object>("SELECT LOGO_WEB_SITE, Nombre FROM SIF_EMPRESA");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error obteniendo información de empresa");
                resp.Code = 0;
                resp.Description = ex.Message;
                resp.Result = new { LOGO_WEB_SITE = DefaultLogoUrl, Nombre = DefaultEmpresa };
            }

            return resp;
        }

        // ======================= API LEGACY (V1) =======================
        public ErrorDto<object> ReporteRDLC(FrmReporteGlobal data)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(data.codEmpresa);
            var error = new ErrorDto<object>();

            try
            {
                using var connection = new SqlConnection(connString);
                var report = CreateReportInstance(data);
                var allDatasets = LoadReportDataSets(data, report.ReportPath);

                var (reportParams, jObject) = BuildReportParameters(data, connection, connString);
                if (reportParams.Count > 0)
                    report.SetParameters(reportParams);

                ProcessDataSets(report, allDatasets, jObject, data, connection, error);

                if (data.cod_reporte == "P")
                {
                    var bytes = report.Render("PDF");
                    error.Result = Convert.ToBase64String(bytes);
                }

                error.Code = 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en ReporteRDLC (legacy)");
                error.Code = 1;
                error.Description = ex.Message;
            }

            return error;
        }

        private LocalReport CreateReportInstance(FrmReporteGlobal data)
        {
            var report = new LocalReport { EnableExternalImages = true };
            var path = Path.Combine(_dirRdlc, data.codEmpresa.ToString(), $"{data.nombreReporte}.rdlc");
            report.ReportPath = path;
            return report;
        }

        private List<(string ReportName, string DataSetName, string? Query)> LoadReportDataSets(FrmReporteGlobal data, string path)
        {
            var allDatasets = new List<(string ReportName, string DataSetName, string? Query)>();

            var doc = System.Xml.Linq.XDocument.Load(path);
            var mainDatasets = doc.Descendants()
                .Where(x => x.Name.LocalName == "DataSet")
                .Select(ds => (
                    ReportName: data.nombreReporte!,
                    DataSetName: ds.Attribute("Name")?.Value ?? string.Empty,
                    Query: ds.Descendants().FirstOrDefault(q => q.Name.LocalName == "CommandText")?.Value))
                .ToList();

            allDatasets.AddRange(mainDatasets);
            allDatasets.AddRange(LoadSubreportDataSets(doc, data));

            return allDatasets;
        }

        private IEnumerable<(string ReportName, string DataSetName, string? Query)> LoadSubreportDataSets(System.Xml.Linq.XDocument doc, FrmReporteGlobal data)
        {
            var subreportNames = doc.Descendants()
                .Where(x => x.Name.LocalName == "Subreport")
                .Select(x => x.Elements().FirstOrDefault(e => e.Name.LocalName == "ReportName")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();

            foreach (var subreportName in subreportNames)
            {
                var subPath = Path.Combine(_dirRdlc, data.codEmpresa.ToString(), $"{subreportName}.rdlc");
                if (!File.Exists(subPath)) continue;

                var subDoc = System.Xml.Linq.XDocument.Load(subPath);
                foreach (var ds in subDoc.Descendants().Where(x => x.Name.LocalName == "DataSet"))
                {
                    yield return (
                        ReportName: subreportName!,
                        DataSetName: ds.Attribute("Name")?.Value ?? string.Empty,
                        Query: ds.Descendants().FirstOrDefault(q => q.Name.LocalName == "CommandText")?.Value
                    );
                }
            }
        }

        private static (List<ReportParameter> Params, JObject? JObject) BuildReportParameters(FrmReporteGlobal data, SqlConnection connection, string connString)
        {
            var reporteParametros = new List<ReportParameter>();
            JObject? jObject = null;

            if (!string.IsNullOrWhiteSpace(data.parametros))
            {
                jObject = JObject.Parse(data.parametros);
                foreach (var prop in jObject.Properties().Where(p => !p.Name.Equals("urlLogo", StringComparison.OrdinalIgnoreCase)))
                    reporteParametros.Add(new ReportParameter(prop.Name, prop.Value?.ToString() ?? string.Empty));

                if (data.parametros.Contains("conString", StringComparison.OrdinalIgnoreCase))
                    reporteParametros.Add(new ReportParameter("conString", connString));

                if (data.parametros.Contains("urlLogo", StringComparison.OrdinalIgnoreCase))
                {
                    var logo = connection.Query<string>("SELECT LOGO_WEB_SITE FROM SIF_EMPRESA").FirstOrDefault();
                    reporteParametros.Add(new ReportParameter("urlLogo", logo ?? string.Empty));
                }
            }

            return (reporteParametros, jObject);
        }

        private static void ProcessDataSets(LocalReport report,
                                           IEnumerable<(string ReportName, string DataSetName, string? Query)> allDatasets,
                                           JObject? jObject,
                                           FrmReporteGlobal data,
                                           SqlConnection connection,
                                           ErrorDto<object> error)
        {
            foreach (var ds in allDatasets)
            {
                var query = ReplaceQueryParameters(ds.Query, jObject).Result;
                var tabla = connection.Query(query).ToList();

                if (data.cod_reporte == "P")
                    report.DataSources.Add(new ReportDataSource(ds.DataSetName, tabla));
                else
                    error.Result = JsonConvert.SerializeObject(tabla, Formatting.Indented);
            }
        }

        private static async Task<string> ReplaceQueryParameters(string? query, JObject? jObject)
        {
            if (jObject == null || string.IsNullOrEmpty(query))
                return query ?? string.Empty;

            var result = query;
            foreach (var prop in jObject.Properties())
            {
                var nombre = prop.Name;
                var valor = prop.Value?.ToString() ?? string.Empty;
                result = result.Replace($"@{nombre}", string.IsNullOrWhiteSpace(valor) ? "NULL" : $"'{valor}'", StringComparison.Ordinal);
            }

            return result;
        }
    }
}
