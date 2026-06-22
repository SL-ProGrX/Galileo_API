using Dapper;
using Galileo.DataBaseTier.ProGrX_Reportes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security;

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
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;
        private readonly ILogger<MReportingServicesDB>? _logger;

        private readonly IRdlcPathResolver _path;
        private readonly IRdlcMetaReader _meta;
        private readonly IRdlcCodePatcher _patcher;
        private readonly IRdlcExecutor _exec;
        private readonly IReportParameterBuilder _params;
        private readonly ISubreportCoordinator _subs;


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

            _portalDB = new PortalDB(config);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        }

        // ================= API PRINCIPAL (V2) =================
        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
         {
            string _dirRdlc = GetParametrerValues(data.codEmpresa, "Rep01").Result!;
            if (data.parametros == null)
                return ReportRenderer.Error("Datos del reporte no proporcionados.", 400);

            if (string.IsNullOrWhiteSpace(data.nombreReporte))
                return ReportRenderer.Error("El nombre del reporte no puede ser nulo o vacío.", 400);

            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(data.codEmpresa);
            try
            {
                using var connection = new SqlConnection(connString);
                connection.Open();

                // 1) Validar inputs como segmentos
                var reportFile = data.nombreReporte.Trim();
                ValidateSegment(reportFile, nameof(data.nombreReporte), allowEmpty: false);

                ValidateSegment(data.folder, nameof(data.folder), allowEmpty: true);

                var report = new LocalReport { EnableExternalImages = true };
                var basePath = _path.GetBasePath(data.codEmpresa, _dirRdlc, data.folder ?? null);

                var mainPath = _path.CombineUnderRoot(basePath, reportFile);
                var finalPath = _path.ResolveReportPath(data.codEmpresa, mainPath);

                // CxSuppress: PathTraversal
                if (!System.IO.File.Exists(finalPath))
                    return ReportRenderer.Error("No se encontró el reporte principal.", 404);

                using var patched = _patcher.PatchReportCode(finalPath, data.codeSection);
                report.LoadReportDefinition(patched);

                var (mainDataSets, subreportNames) = _meta.ReadRdlcMeta(finalPath);
                var subMeta = _subs.LoadSubreports(data.codEmpresa, report, basePath, subreportNames);
                var autoAliases = _subs.BuildAutoAliasMap(data.codEmpresa, finalPath, basePath);

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

                string errorDesc = string.Join("\n", subErrors) + "\n";

                //elimino ultimo salto de linea
                errorDesc = errorDesc.TrimEnd('\n');

                if (errorDesc.Length > 0)
                {
                   return ReportRenderer.Error($"Errores en subreportes:\n{errorDesc}", 500);
                }

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

            string DefaultLogoUrl = GetParametrerValues(codEmpresa, "Rep02").Result!;
            string DefaultEmpresa = GetParametrerValues(codEmpresa, "Rep03").Result!;
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
            string dirRdlc = GetParametrerValues(data.codEmpresa, "Rep01").Result!;
            var report = new LocalReport { EnableExternalImages = true };

            var reportFile = (data.nombreReporte ?? string.Empty).Trim();
            ValidateSegment(reportFile, nameof(data.nombreReporte), allowEmpty: false);

            var basePath = _path.GetBasePath(data.codEmpresa, dirRdlc, data.folder ?? null);
            var mainPath = _path.CombineUnderRoot(basePath, reportFile);
            var finalPath = _path.ResolveReportPath(data.codEmpresa, mainPath);

            if (string.IsNullOrWhiteSpace(finalPath) || !File.Exists(finalPath))
                throw new FileNotFoundException("No se encontró el reporte principal.");

            report.ReportPath = finalPath;
            return report;
        }

        private List<(string ReportName, string DataSetName, string? Query)> LoadReportDataSets(FrmReporteGlobal data, string path)
        {
            var allDatasets = new List<(string ReportName, string DataSetName, string? Query)>();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return allDatasets;

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
            string dirRdlc = GetParametrerValues(data.codEmpresa, "Rep01").Result!;

            var basePath = _path.GetBasePath(data.codEmpresa, dirRdlc, data.folder ?? null);

            var subreportNames = doc.Descendants()
                .Where(x => x.Name.LocalName == "Subreport")
                .Select(x => x.Elements().FirstOrDefault(e => e.Name.LocalName == "ReportName")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();

            foreach (var subreportName in subreportNames)
            {
                ValidateSegment(subreportName, nameof(subreportName), allowEmpty: false);

                var mainPath = _path.CombineUnderRoot(basePath, subreportName ?? string.Empty);
                var subPath = _path.ResolveReportPath(data.codEmpresa, mainPath);

                if (string.IsNullOrWhiteSpace(subPath) || !File.Exists(subPath))
                    continue;

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

        private static Task<string> ReplaceQueryParameters(string? query, JObject? jObject)
        {
            if (jObject == null || string.IsNullOrEmpty(query))
                return Task.FromResult(query ?? string.Empty);

            var result = query;
            foreach (var prop in jObject.Properties())
            {
                var nombre = prop.Name;
                var valor = prop.Value?.ToString() ?? string.Empty;
                result = result.Replace($"@{nombre}", string.IsNullOrWhiteSpace(valor) ? "NULL" : $"'{valor}'", StringComparison.Ordinal);
            }

            return Task.FromResult(result);
        }

        private ErrorDto<string> GetParametrerValues(int CodEmpresa, string code)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @Code";
                return conn.Query<string>(query, new { Code = code }).FirstOrDefault() ?? string.Empty;
            });
        }

        static void ValidateSegment(string? segment, string paramName, bool allowEmpty)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                if (allowEmpty) return;
                throw new SecurityException($"{paramName} requerido.");
            }

            // No permitir separadores => no subcarpetas, no traversal con / o \
            if (segment.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
                throw new SecurityException($"{paramName} debe ser un solo segmento.");

            if (segment is "." or ".." || segment.Contains("..", StringComparison.Ordinal))
                throw new SecurityException($"{paramName} inválido.");

            if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new SecurityException($"{paramName} contiene caracteres inválidos.");
        }
    }
}
