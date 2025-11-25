using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Servicio que renderiza RDLC (principal + subreportes) dinámicamente.
    /// - Soporta DataSets con SQL o Stored Procedures (CommandType del RDLC o heurística).
    /// - Evita parámetros duplicados.
    /// - Para SP, filtra los parámetros a los que realmente existen en el SP (sys.parameters).
    /// - Maneja SubreportProcessing de forma resiliente (no rompe el render si un DS falla).
    /// - Construye automáticamente alias de parámetros entre Padre y Subreportes.
    /// - NUEVO: Soporta data.codeSection para inyectar funciones en <Code> (a partir de JSON o VB crudo)
    ///   y evitar consultas de subreportes cuando estén ocultos por funciones (fxImprimeDetalle/Ref).
    /// </summary>
    public class mReportingServicesDB
    {
        private readonly IConfiguration _config;
        private readonly string dirRDLC = "";

        public mReportingServicesDB(IConfiguration config)
        {
            _config = config;
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value?.ToString() ?? "";
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        // ------------------ API PRINCIPAL ------------------
        public IActionResult ReporteRDLC_v2(FrmReporteGlobal data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(data.codEmpresa);

            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    connection.Open();

                    var report = new LocalReport
                    {
                        EnableExternalImages = true
                    };

                    var subErrors = new List<string>();
                    var jsonDataSets = new Dictionary<string, object>(); // <== te faltaba declararlo aquí

                    string basePath = (data.folder != null)
                        ? Path.Combine(dirRDLC, data.codEmpresa.ToString(), data.folder.ToString())
                        : Path.Combine(dirRDLC, data.codEmpresa.ToString());

                    string? mainPath = ResolveReportPath(basePath, data.nombreReporte ?? string.Empty);
                    if (mainPath == null)
                    {
                        return new ObjectResult(new { Code = -1, Description = $"No se encontró el reporte principal (.rdlc|.rdl): {Path.Combine(basePath, data.nombreReporte ?? string.Empty)}" }) { StatusCode = 500 };
                    }

                    // Solo informativo: funciones existentes en el RDLC (antes del parche)
                    var funcsInReport = GetFunctionsFromReportFile(mainPath);

                    // ---------- NUEVO: parchear <Code> en memoria según data.codeSection ----------
                    using (var patched = PatchReportCode(mainPath, data.codeSection))
                        report.LoadReportDefinition(patched);

                    // Para gateo de subreportes: intenta obtener constantes de retorno para fxImprimeDetalle/Ref
                    int? fxDetConst = null, fxRefConst = null;
                    if (!string.IsNullOrWhiteSpace(data.codeSection))
                    {
                        // ¿JSON?
                        try
                        {
                            var jo = JObject.Parse(data.codeSection);
                            fxDetConst = TryParseFlag(jo, "fxImprimeDetalle");
                            fxRefConst = TryParseFlag(jo, "fxImprimeRef");
                        }
                        catch
                        {
                            // VB crudo
                            fxDetConst = GetCodeFunctionConstantReturnFromText(data.codeSection, "fxImprimeDetalle");
                            fxRefConst = GetCodeFunctionConstantReturnFromText(data.codeSection, "fxImprimeRef");
                        }
                    }

                    // También obtenemos lista de funciones declaradas en codeSection (JSON o VB crudo) - opcional/informativo
                    var (funcsFromCodeSection, _) = GetFunctionsFromCodeSection(data.codeSection);

                    // ==== 1) Leer meta del principal (datasets y subreportes) ====
                    var (mainDataSets, subreportNames) = ReadRdlcMeta(mainPath);

                    // ==== 2) Cargar definiciones y meta de subreportes ====
                    var subMeta = new Dictionary<string, List<RdlcDataSetMeta>>(StringComparer.OrdinalIgnoreCase);
                    foreach (var subName in subreportNames)
                    {
                        // Soporte para rutas con carpetas en ReportName (ej: "Carpeta/Sub")
                        var subPath = ResolveReportPath(basePath, subName);
                        if (subPath == null) { /* opcional: log/skip */ continue; }

                        using (var fs = File.OpenRead(subPath))
                            report.LoadSubreportDefinition(subName, fs);

                        var (subDs, _) = ReadRdlcMeta(subPath);
                        subMeta[subName] = subDs;
                    }

                    // ==== 2.1) Construir alias automáticos Padre->Hijo por subreporte ====
                    var autoAliases = BuildAutoAliasMap(mainPath, basePath);

                    // ==== 3) Parámetros del reporte ====
                    var reporteParametros = new List<ReportParameter>();
                    JObject? jObject = null;
                    var paramDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (!string.IsNullOrWhiteSpace(data.parametros))
                    {
                        jObject = JObject.Parse(data.parametros);

                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Name.Equals("urlLogo", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (prop.Name.Equals("Empresa", StringComparison.OrdinalIgnoreCase))
                                continue;

                            var val = prop.Value?.ToString() ?? "";
                            reporteParametros.Add(new ReportParameter(prop.Name, val));
                            paramDict[prop.Name] = val;
                        }

                        if (data.parametros.Contains("conString", StringComparison.OrdinalIgnoreCase))
                        {
                            reporteParametros.Add(new ReportParameter("conString", stringConn));
                            paramDict["conString"] = stringConn;
                        }

                        if (data.parametros.Contains("urlLogo", StringComparison.OrdinalIgnoreCase))
                        {
                            var logo = connection.Query<string>("SELECT LOGO_WEB_SITE FROM SIF_EMPRESA").FirstOrDefault();
                            reporteParametros.Add(new ReportParameter("urlLogo", logo ?? ""));
                            paramDict["urlLogo"] = logo ?? "";
                        }

                        if (data.parametros.Contains("Empresa", StringComparison.OrdinalIgnoreCase))
                        {
                            var nombreEmpresa = connection.Query<string>("SELECT Nombre FROM SIF_EMPRESA").FirstOrDefault();
                            reporteParametros.Add(new ReportParameter("Empresa", nombreEmpresa ?? ""));
                            paramDict["Empresa"] = nombreEmpresa ?? "";
                        }

                        report.SetParameters(reporteParametros);
                    }

                    // Para colectar errores de subreportes sin romper el render

                    // ==== 4) Datasets del principal ====
                    foreach (var ds in mainDataSets)
                    {
                        IEnumerable<object> rows;

                        if (!string.IsNullOrWhiteSpace(ds.CommandText))
                        {
                            if (!TryExecDataSet(
                                    connection,
                                    ds,
                                    ctx: paramDict,
                                    jsonParams: jObject ?? [],
                                    allowFiltrosReplacement: true,
                                    out rows,
                                    out var err,
                                    emitExecAsTextForSp: false
                                ))
                            {
                                rows = Enumerable.Empty<object>();
                                if (!string.IsNullOrWhiteSpace(err))
                                    subErrors.Add($"[MAIN DS '{ds.DataSetName}'] {err}");
                            }
                        }
                        else
                        {
                            rows = Enumerable.Empty<object>();
                        }

                        if (data.cod_reporte == "P")
                            report.DataSources.Add(new ReportDataSource(ds.DataSetName, rows));
                        else
                        {
                            // para JSON: devolvemos por dataset
                            if (!string.IsNullOrEmpty(ds.DataSetName))
                                jsonDataSets[ds.DataSetName] = rows;
                        }
                    }

                    // ==== 5) SubreportProcessing: alimentar cada subreporte ====
                    report.SubreportProcessing += (sender, e) =>
                    {
                        try
                        {
                            var subName = e.ReportPath; // Debe coincidir con el ReportName del control subreporte
                            if (!subMeta.TryGetValue(subName, out var datasets) || datasets == null)
                                return;

                            // GATE opcional por codeSection: si fxImprimeRef/Detalle = 0, NO ejecutamos el subreporte.
                            // Heurística por nombre: "Ref" -> fxImprimeRef, "Det/Detalle" -> fxImprimeDetalle.
                            bool skip = false;
                            if (fxRefConst.HasValue && subName.IndexOf("ref", StringComparison.OrdinalIgnoreCase) >= 0)
                                skip |= (fxRefConst.Value == 0);
                            if (fxDetConst.HasValue &&
                                (subName.IndexOf("det", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 subName.IndexOf("detalle", StringComparison.OrdinalIgnoreCase) >= 0))
                                skip |= (fxDetConst.Value == 0);

                            if (skip)
                            {
                                foreach (var dsd in datasets)
                                    e.DataSources.Add(new ReportDataSource(dsd.DataSetName, Enumerable.Empty<object>()));
                                return;
                            }

                            // 1) Mezcla parámetros globales + los que el padre pasa al subreporte (POR FILA)
                            var merged = new Dictionary<string, string>(paramDict, StringComparer.OrdinalIgnoreCase);
                            var receivedFromParent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (ReportParameterInfo p in e.Parameters)
                            {
                                var val = p.Values?.FirstOrDefault() ?? "";
                                merged[p.Name] = val;
                                receivedFromParent[p.Name] = val;
                            }

                            // 2) Aplica alias auto-leídos: (paramDelPadre -> paramEsperadoHijo)
                            if (autoAliases.TryGetValue(subName, out var aliasMap))
                            {
                                foreach (var kv in aliasMap)
                                {
                                    if (!merged.ContainsKey(kv.Value) && receivedFromParent.TryGetValue(kv.Key, out var v))
                                        merged[kv.Value] = v;
                                }
                            }

                            // 3) Validar que el hijo realmente tenga sus parámetros cubiertos
                            var expected = datasets
                                .SelectMany(ds => ds.QueryParams)
                                .Select(qp =>
                                {
                                    var m = Regex.Match(
                                                qp.ValueExpr ?? "",
                                                @"^=Parameters!(?<p>\w+)\.Value$",
                                                RegexOptions.IgnoreCase,
                                                TimeSpan.FromSeconds(1) // timeout
                                            );
                                    return m.Success ? m.Groups["p"].Value : null;
                                })
                                .Where(n => !string.IsNullOrWhiteSpace(n))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                            // Fallback: si el subreporte espera 1 parámetro y el padre pasó 1 con nombre raro
                            if (expected.Count == 1 && receivedFromParent.Count == 1 && expected[0] != null)
                            {
                                var key = expected[0];
                                if (key != null && !merged.ContainsKey(key))
                                    merged[key] = receivedFromParent.Values.FirstOrDefault() ?? "";
                            }

                            var missing = expected
                                .Where(n => n != null && !merged.ContainsKey(n))
                                .ToList();
                            if (missing.Count > 0)
                                throw new InvalidOperationException(
                                    $"El subreporte '{subName}' requiere parámetro(s) {string.Join(", ", missing)}. " +
                                    $"Mapéalos desde el padre (Name='{missing[0]}' Value =Fields!X.Value).");

                            // 4) Ejecuta datasets del subreporte con los parámetros POR FILA ya mapeados
                            foreach (var ds in datasets)
                            {
                                if (string.IsNullOrWhiteSpace(ds.DataSetName))
                                    continue;

                                if (!string.IsNullOrWhiteSpace(ds.CommandText))
                                {
                                    if (!TryExecDataSet(
                                            connection,
                                            ds,
                                            ctx: merged,
                                            jsonParams: JObject.FromObject(merged),
                                            allowFiltrosReplacement: false,
                                            out var rows,
                                            out var err,
                                            emitExecAsTextForSp: false
                                        ))
                                    {
                                        if (!string.IsNullOrWhiteSpace(err))
                                            subErrors.Add($"[SUB '{subName}' DS '{ds.DataSetName}'] {err}");
                                        e.DataSources.Add(new ReportDataSource(ds.DataSetName, Enumerable.Empty<object>()));
                                    }
                                    else
                                    {
                                        e.DataSources.Add(new ReportDataSource(ds.DataSetName, rows));

                                    }
                                }
                                else
                                {
                                    e.DataSources.Add(new ReportDataSource(ds.DataSetName, Enumerable.Empty<object>()));
                                }
                            }
                        }
                        catch (Exception exSub)
                        {
                            subErrors.Add($"[SUB '{e.ReportPath}'] {exSub.GetType().Name}: {exSub.Message}");
                            //return new ObjectResult(new { Code = -1, Description = exSub.Message }) { StatusCode = 500 };
                        }
                    };

                    // ==== 6) Render ====
                    if (data.cod_reporte == "P")
                    {
                        Warning[] warnings;
                        string mimeType, encoding, extension;
                        string[] streamIds;

                        var bytes = report.Render(
                            "PDF",
                            null,
                            out mimeType, out encoding, out extension,
                            out streamIds, out warnings
                        );

                        // Devuelve PDF sin usar Response ni helpers
                        var fileResult = new FileContentResult(bytes, "application/pdf")
                        {
                            FileDownloadName = (data.nombreReporte ?? "reporte") + ".pdf" // esto sugiere "attachment"
                        };

                        return fileResult;
                    }
                    else
                    {
                        // Devuelve JSON con datasets + advertencias
                        var payload = new
                        {
                            Code = 0,
                            DataSets = jsonDataSets,
                            Warnings = subErrors
                        };
                        return new OkObjectResult(payload);
                    }

                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { Code = -1, Description = ex.Message }) { StatusCode = 500 };
            }

        }

        #region VERSION 2 HELPERS
        // ------------------ HELPERS ------------------

        private sealed class RdlcDataSetMeta
        {
            public string? DataSetName { get; set; }
            public string? CommandText { get; set; }                // nombre SP o SQL
            public string? CommandType { get; set; }                // "StoredProcedure" | "Text" | null
            public List<(string Name, string ValueExpr)> QueryParams { get; } = new();
        }

        /// <summary>
        /// Lee datasets (nombre, CommandText, CommandType, QueryParameters) y nombres de subreportes (ReportName) desde un RDLC.
        /// </summary>
        private static (List<RdlcDataSetMeta> dataSets, List<string> subreportNames) ReadRdlcMeta(string rdlcPath)
        {
            var xdoc = XDocument.Load(rdlcPath);
            if (xdoc.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            if (xdoc.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            XNamespace ns = xdoc.Root.GetDefaultNamespace();

            var dataSets = xdoc.Descendants(ns + "DataSet")
                .Select(ds => new RdlcDataSetMeta
                {
                    DataSetName = (string?)ds.Attribute("Name") ?? string.Empty,
                    CommandText = ds.Descendants(ns + "CommandText").FirstOrDefault()?.Value,
                    CommandType = ds.Descendants(ns + "CommandType").FirstOrDefault()?.Value
                })
                .ToList();

            foreach (var ds in xdoc.Descendants(ns + "DataSet"))
            {
                var name = (string?)ds.Attribute("Name") ?? string.Empty;
                var meta = dataSets.FirstOrDefault(m => m.DataSetName == name);
                var qps = ds.Descendants(ns + "QueryParameters").FirstOrDefault();
                if (qps != null && meta != null)
                {
                    foreach (var qp in qps.Elements(ns + "QueryParameter"))
                    {
                        meta.QueryParams.Add((
                            qp.Attribute("Name")?.Value ?? string.Empty,
                            qp.Element(ns + "Value")?.Value ?? string.Empty // p.ej. "=Parameters!Id.Value"
                        ));
                    }
                }
            }

            var subreports = xdoc.Descendants(ns + "Subreport")
                .Select(s => s.Element(ns + "ReportName")?.Value)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>() // Ensures all are non-null
                .ToList();

            return (dataSets, subreports);
        }

        /// <summary>
        /// Evalúa expresiones simples del estilo "=Parameters!X.Value" usando el diccionario de parámetros.
        /// Soporta literales numéricos y de texto.
        /// </summary>
        private static object EvalExpr(string expr, IDictionary<string, string> ctx)
        {
            if (string.IsNullOrWhiteSpace(expr)) return DBNull.Value;

            var e = expr.Trim();

            var m = Regex.Match(
                    e,
                    @"^=Parameters!(?<p>\w+)\.Value$",
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromSeconds(1)  // timeout recomendado
                );

            if (m.Success)
            {
                var pname = m.Groups["p"].Value;
                return ctx.TryGetValue(pname, out var v) ? (object)(v ?? "") : DBNull.Value;
            }

            if (decimal.TryParse(e, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;

            return e.Trim('"', '\'');
        }

        private static string NormalizeParamName(string name) => name?.Trim().TrimStart('@') ?? "";

        private static bool LooksLikeSpName(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return false;
            var q = commandText.Trim();

            if (q.IndexOf(' ') >= 0) return false;
            if (q.StartsWith("select", StringComparison.OrdinalIgnoreCase)) return false;
            if (q.StartsWith("with", StringComparison.OrdinalIgnoreCase)) return false;
            if (q.StartsWith("insert", StringComparison.OrdinalIgnoreCase)) return false;
            if (q.StartsWith("update", StringComparison.OrdinalIgnoreCase)) return false;
            if (q.StartsWith("delete", StringComparison.OrdinalIgnoreCase)) return false;

            return true; // nombre "limpio" => parece SP
        }

        private static HashSet<string> GetStoredProcedureParamNames(SqlConnection conn, string spName)
        {
            const string sql = @"
SELECT REPLACE(p.name,'@','') AS ParamName
FROM sys.parameters p
WHERE p.object_id = OBJECT_ID(@proc)
ORDER BY p.parameter_id;
";
            try
            {
                var list = conn.Query<string>(sql, new { proc = spName }).ToList();
                return new HashSet<string>(list.Select(NormalizeParamName), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static bool TryExecDataSet(
            SqlConnection connection,
            RdlcDataSetMeta ds,
            IDictionary<string, string> ctx,
            JObject jsonParams,
            bool allowFiltrosReplacement,
            out IEnumerable<object> rows,
            out string error,
            bool emitExecAsTextForSp = false
        )
        {
            rows = Enumerable.Empty<object>();
            error = string.Empty;

            try
            {
                var paramPairs = new List<(string Name, object? Value)>();
                foreach (var (qpName, expr) in ds.QueryParams)
                {
                    var raw = EvalExpr(expr, ctx);
                    paramPairs.Add((qpName, CoerceEmptyToNull(raw)));
                }

                var existing = new HashSet<string>(paramPairs.Select(p => NormalizeParamName(p.Name)),
                                                   StringComparer.OrdinalIgnoreCase);

                if (jsonParams != null)
                {
                    foreach (var prop in jsonParams.Properties())
                    {
                        var name = prop.Name;
                        if (string.Equals(name, "filtros", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!existing.Contains(name))
                        {
                            var val = prop.Value?.ToString();
                            paramPairs.Add((name, string.IsNullOrWhiteSpace(val) ? null : val));
                            existing.Add(name);
                        }
                    }
                }

                bool isStoredProc =
                      string.Equals(ds.CommandType, "StoredProcedure", StringComparison.OrdinalIgnoreCase)
                   || (string.IsNullOrWhiteSpace(ds.CommandType) && !string.IsNullOrWhiteSpace(ds.CommandText) && LooksLikeSpName(ds.CommandText));

                string sqlText = ds.CommandText?.Trim() ?? "";
                bool isExecBatch = sqlText.StartsWith("exec ", StringComparison.OrdinalIgnoreCase);

                if (!isStoredProc && allowFiltrosReplacement && jsonParams != null && jsonParams.TryGetValue("filtros", out var filtrosToken))
                {
                    var f = filtrosToken?.ToString();
                    if (!string.IsNullOrWhiteSpace(f))
                        sqlText = sqlText.Replace("@filtros", f);
                }

                if (isStoredProc)
                {
                    var spName = sqlText;
                    if (isExecBatch)
                    {
                        var parts = sqlText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        spName = parts.Length > 1 ? parts[1] : sqlText;
                    }

                    var allowed = GetStoredProcedureParamNames(connection, spName);
                    if (allowed.Count == 0)
                    {
                        allowed = new HashSet<string>(
                            ds.QueryParams.Select(q => NormalizeParamName(q.Name)),
                            StringComparer.OrdinalIgnoreCase
                        );
                    }

                    paramPairs = paramPairs
                        .Where(pp => allowed.Contains(NormalizeParamName(pp.Name)))
                        .ToList();
                }

                var dp = new DynamicParameters();
                foreach (var (name, val) in paramPairs)
                {
                    dp.Add("@" + NormalizeParamName(name), val);
                }

                if (isStoredProc && !isExecBatch && !emitExecAsTextForSp)
                {
                    var spName = sqlText;

                    var result = connection.Query(spName, dp, commandType: CommandType.StoredProcedure).ToList();
                    if (result.Count > 0)
                    {
                        rows = result;
                        return true;
                    }

                    using (var reader = connection.ExecuteReader(spName, dp, commandType: CommandType.StoredProcedure))
                    {
                        var schema = reader.GetSchemaTable();
                        if (schema == null || schema.Rows.Count == 0)
                        {
                            rows = Enumerable.Empty<object>();
                            return true;
                        }

                        var empty = new ExpandoObject() as IDictionary<string, object>;
                        foreach (DataRow col in schema.Rows)
                        {
                            var colName = col["ColumnName"]?.ToString();
                            if (!string.IsNullOrEmpty(colName) && !empty.ContainsKey(colName))
                                empty[colName] = (object)null!;
                        }

                        rows = new List<object> { (object)empty };
                        return true;
                    }
                }
                else if (isStoredProc && emitExecAsTextForSp)
                {
                    var spName = sqlText;
                    if (isExecBatch)
                    {
                        var parts = sqlText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        spName = parts.Length > 1 ? parts[1] : sqlText;
                    }
                    var execText = BuildExecBatchText(spName, paramPairs);
                    rows = connection.Query(execText, commandType: CommandType.Text).ToList();
                    return true;
                }
                else
                {
                    rows = connection.Query(sqlText, dp, commandType: CommandType.Text).ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = $"[{ds.DataSetName}] {ex.GetType().Name}: {ex.Message}";
                rows = Enumerable.Empty<object>();
                return false;
            }
        }

        private static string BuildExecBatchText(string spName, IEnumerable<(string Name, object? Value)> parameters)
        {
            var parts = new List<string>();
            foreach (var (name, value) in parameters)
            {
                var lit = ToSqlLiteral(value);
                parts.Add($"@{NormalizeParamName(name)}={lit}");
            }
            var args = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(args) ? $"EXEC {spName}" : $"EXEC {spName} {args}";
        }

        private static string ToSqlLiteral(object? value)
        {
            if (value == null || value is DBNull) return "NULL";

            switch (value)
            {
                case bool b: return b ? "1" : "0";
                case byte or sbyte or short or ushort or int or uint or long or ulong:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                case float or double or decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                case DateTime dt:
                    return $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'";
                case DateTimeOffset dto:
                    return $"'{dto:yyyy-MM-dd HH:mm:ss.fff zzz}'";
                case Guid g:
                    return $"'{g}'";
                default:
                    var s = Convert.ToString(value);
                    s = s?.Replace("'", "''");
                    return $"N'{s}'";
            }
        }

        private static object? CoerceEmptyToNull(object? val)
        {
            if (val is null) return null;

            if (val is Newtonsoft.Json.Linq.JValue jv)
            {
                if (jv.Type == Newtonsoft.Json.Linq.JTokenType.Null) return null;
                val = jv.Value;
                if (val is null) return null;
            }

            if (val is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase)) return null;
                return s;
            }

            return val;
        }

        // ------------------ ALIAS AUTO: PADRE ↔ HIJO ------------------

        private static Dictionary<string, Dictionary<string, string>> BuildAutoAliasMap(string parentRdlcPath, string basePath)
        {
            var parentMap = ReadParentSubreportParamNames(parentRdlcPath);
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in parentMap)
            {
                var subName = kv.Key;
                var parentParamNames = kv.Value;

                var childPath = ResolveReportPath(basePath, subName);
                if (childPath == null) continue;

                var childDoc = XDocument.Load(childPath);
                var expected = ReadExpectedChildParams(childDoc).ToList();

                var alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (expected.Count == 1)
                {
                    foreach (var p in parentParamNames)
                        alias[p] = expected[0];
                }
                else
                {
                    var expNorm = expected.ToDictionary(Norm, e => e, StringComparer.OrdinalIgnoreCase);

                    foreach (var parentName in parentParamNames)
                    {
                        var np = Norm(parentName);
                        if (expNorm.TryGetValue(np, out var hit))
                        {
                            alias[parentName] = hit;
                            continue;
                        }

                        var hit2 = expected.FirstOrDefault(e => Norm(e).EndsWith(np) || np.EndsWith(Norm(e)));
                        if (!string.IsNullOrEmpty(hit2))
                        {
                            alias[parentName] = hit2;
                        }
                    }

                    if (alias.Count < Math.Min(parentParamNames.Count, expected.Count)
                        && parentParamNames.Count == expected.Count)
                    {
                        for (int i = 0; i < parentParamNames.Count; i++)
                        {
                            var p = parentParamNames[i];
                            if (!alias.ContainsKey(p))
                                alias[p] = expected[i];
                        }
                    }
                }

                if (alias.Count > 0)
                    result[subName] = alias;
            }

            return result;
        }

        private static Dictionary<string, List<string>> ReadParentSubreportParamNames(string parentRdlcPath)
        {
            var x = XDocument.Load(parentRdlcPath);
            if (x.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            XNamespace ns = x.Root.GetDefaultNamespace();

            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var sr in x.Descendants(ns + "Subreport"))
            {
                var reportName = sr.Element(ns + "ReportName")?.Value;
                if (string.IsNullOrWhiteSpace(reportName)) continue;

                var list = new List<string>();
                var ps = sr.Element(ns + "Parameters");
                if (ps != null)
                {
                    foreach (var p in ps.Elements(ns + "Parameter"))
                    {
                        var nameAttr = p.Attribute("Name")?.Value;
                        if (!string.IsNullOrWhiteSpace(nameAttr)) list.Add(nameAttr);
                    }
                }
                map[reportName] = list;
            }

            return map;
        }

        private static HashSet<string> ReadExpectedChildParams(XDocument childDoc)
        {
            if (childDoc.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            XNamespace ns = childDoc.Root.GetDefaultNamespace();
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rp in childDoc.Descendants(ns + "ReportParameter"))
            {
                var name = rp.Attribute("Name")?.Value;
                if (!string.IsNullOrWhiteSpace(name)) expected.Add(name);
            }

            foreach (var qp in childDoc.Descendants(ns + "QueryParameter"))
            {
                var valExpr = qp.Element(ns + "Value")?.Value;
                if (string.IsNullOrWhiteSpace(valExpr)) continue;

               var m = Regex.Match(
                            valExpr.Trim(),
                            @"^=Parameters!(?<p>\w+)\.Value$",
                            RegexOptions.IgnoreCase,
                            TimeSpan.FromSeconds(1) // timeout recomendado
                        );

                if (m.Success) expected.Add(m.Groups["p"].Value);
            }

            return expected;
        }

        private static string Norm(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var last = s.Split(new[] { '.', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? s;
            var only = new string(last.Where(char.IsLetterOrDigit).ToArray());
            return only.ToUpperInvariant();
        }

        // ------------------ NUEVO: Soporte codeSection ------------------

        // Inserta o reemplaza la función con un Return constante dentro del texto VB del <Code>.
        private static string UpsertFunctionReturn(string codeText, string funcName, int ret)
        {
          var funcBlock = new Regex(
                                $@"(?is)Public\s+Function\s+{Regex.Escape(funcName)}\s*\(\s*\)\s+As\s+\w+.*?End\s+Function",
                                RegexOptions.None,
                                TimeSpan.FromSeconds(1)
                            );

            if (funcBlock.IsMatch(codeText))
            {
                codeText = funcBlock.Replace(codeText, m =>
                            {
                                var body = m.Value;

                                // Replace con timeout
                                var withReturn = Regex.Replace(
                                    body,
                                    @"(?im)^\s*Return\s+.*$",
                                    $"    Return {ret}",
                                    RegexOptions.Multiline | RegexOptions.IgnoreCase,
                                    TimeSpan.FromSeconds(1)
                                );

                                // IsMatch con timeout
                                if (!Regex.IsMatch(
                                    withReturn,
                                    @"(?im)^\s*Return\s+",
                                    RegexOptions.Multiline | RegexOptions.IgnoreCase,
                                    TimeSpan.FromSeconds(1)
                                ))
                                {
                                    // Segundo Replace con timeout
                                    withReturn = Regex.Replace(
                                        withReturn,
                                        @"(?i)End\s+Function",
                                        $"    Return {ret}\nEnd Function",
                                        RegexOptions.IgnoreCase,
                                        TimeSpan.FromSeconds(1)
                                    );
                                }

                                return withReturn;
                            }, 1);

            }
            else
            {
                codeText += $@"

Public Function {funcName}() As Decimal
    Return {ret}
End Function
";
            }

            return codeText;
        }

        // Parcha el <Code> del RDLC padre:
        // - Si codeSection es VB crudo => reemplaza el <Code> por ese VB.
        // - Si codeSection es JSON => upsert de TODAS las funciones booleanas/numéricas como retornos constantes.
        // - Si codeSection es null => devuelve el RDLC sin cambios.
        private static MemoryStream PatchReportCode(string rdlcPath, string? codeSection)
        {
            var xdoc = XDocument.Load(rdlcPath);
            if (xdoc.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            XNamespace ns = xdoc.Root.GetDefaultNamespace();

            var codeNode = xdoc.Descendants(ns + "Code").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(codeSection))
            {
                var ms0 = new MemoryStream();
                xdoc.Save(ms0); ms0.Position = 0;
                return ms0;
            }

            // ¿JSON con claves=>constantes?
            Dictionary<string, int>? constFuncs = null;
            bool isJson = false;
            try
            {
                constFuncs = ParseConstantFunctionsFromJson(codeSection);
                isJson = true;
            }
            catch
            {
                isJson = false;
            }

            if (!isJson)
            {
                // VB crudo: reemplaza completamente (o crea) el nodo <Code>
                if (codeNode == null)
                {
                    codeNode = new XElement(ns + "Code", codeSection);
                    xdoc.Root.Add(codeNode);
                }
                else
                {
                    codeNode.Value = codeSection;
                }
            }
            else
            {
                // JSON: upsert de funciones con retorno constante para cada key booleana/numérica
                var codeText = codeNode?.Value ?? string.Empty;

                if (constFuncs != null)
                {
                    foreach (var kv in constFuncs)
                        codeText = UpsertFunctionReturn(codeText, kv.Key, kv.Value);
                }

                if (codeNode == null)
                {
                    codeNode = new XElement(ns + "Code", codeText);
                    xdoc.Root.Add(codeNode);
                }
                else
                {
                    codeNode.Value = codeText;
                }
            }

            var ms = new MemoryStream();
            xdoc.Save(ms);
            ms.Position = 0;
            return ms;
        }

        // Si recibiste VB crudo, intenta leer "Return N" constante de la función.
        private static int? GetCodeFunctionConstantReturnFromText(string codeText, string funcName)
        {
            var pattern =
                $@"Public\s+Function\s+{Regex.Escape(funcName)}\s*\(\s*\)\s+As\s+\w+.*?Return\s+(?<ret>-?\d+)\s*[\r\n]+End\s+Function";

            var m = Regex.Match(
                codeText ?? "",
                pattern,
                RegexOptions.Singleline | RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1)
            );

            if (!m.Success) return null;

            if (int.TryParse(m.Groups["ret"].Value, out var val))
                return val;

            return null;
        }


        private static string? ResolveReportPath(string basePath, string reportNameOrRelative)
        {
            // "Folder/SubReport" -> usa separadores del SO
            var rel = reportNameOrRelative
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var dir = Path.Combine(basePath, Path.GetDirectoryName(rel) ?? string.Empty);
            var bare = Path.GetFileName(rel);

            // Candidatos directos
            var candidates = new[]
            {
                    Path.Combine(basePath, rel + ".rdlc"),
                    Path.Combine(basePath, rel + ".rdl"),
                    Path.Combine(dir,      bare + ".rdlc"),
                    Path.Combine(dir,      bare + ".rdl"),
             };
            foreach (var c in candidates)
                if (File.Exists(c)) return c;

            // Búsqueda tolerante a mayúsculas/minúsculas y extensión
            if (Directory.Exists(dir))
            {
                var match = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f =>
                        string.Equals(Path.GetFileNameWithoutExtension(f), bare, StringComparison.OrdinalIgnoreCase) &&
                        (string.Equals(Path.GetExtension(f), ".rdlc", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(Path.GetExtension(f), ".rdl", StringComparison.OrdinalIgnoreCase)));
                if (match != null) return match;
            }

            return null; // no encontrado
        }

        // --------- NUEVO: utilidades para listar funciones en Reporte/CodeSection ---------

        private sealed class VbFunctionSig
        {
            public string Name { get; set; } = "";
            public string ReturnType { get; set; } = "Decimal";
            public List<string> Parameters { get; set; } = new();
        }

        private static List<VbFunctionSig> GetFunctionsFromReportFile(string rdlcPath)
        {
            var xdoc = XDocument.Load(rdlcPath);
            if (xdoc.Root == null)
                throw new InvalidOperationException("The RDLC file does not have a root element.");
            XNamespace ns = xdoc.Root.GetDefaultNamespace();
            var code = xdoc.Descendants(ns + "Code").FirstOrDefault()?.Value ?? "";
            return ParseVbFunctions(code);
        }

        private static (List<VbFunctionSig> functions, bool isJson) GetFunctionsFromCodeSection(string? codeSection)
        {
            if (string.IsNullOrWhiteSpace(codeSection))
                return (new List<VbFunctionSig>(), false);

            // ¿JSON? Los keys son nombres de función; inferimos tipo de retorno.
            try
            {
                var jo = JObject.Parse(codeSection);
                var list = jo.Properties()
                             .Select(p => new VbFunctionSig
                             {
                                 Name = p.Name,
                                 ReturnType = InferReturnTypeFromToken(p.Value),
                                 Parameters = new List<string>()
                             })
                             .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                             .Select(g => g.First())
                             .ToList();
                return (list, true);
            }
            catch
            {
                // VB crudo: parseamos firmas
                return (ParseVbFunctions(codeSection), false);
            }
        }

        private static string InferReturnTypeFromToken(JToken token)
        {
            return token?.Type switch
            {
                JTokenType.Boolean => "Boolean",
                JTokenType.Integer or JTokenType.Float => "Decimal",
                JTokenType.Null or JTokenType.Undefined => "Decimal",
                _ => "String"
            };
        }

        private static List<VbFunctionSig> ParseVbFunctions(string vbCode)
        {
            var result = new List<VbFunctionSig>();
            if (string.IsNullOrWhiteSpace(vbCode)) return result;

            // Firma típica:
            // [Public|Private] [Shared] Function Nombre(param1 As T, param2 As T) As Tipo
         var rx = new Regex(
                    @"(?im)^\s*(Public|Private)?\s*(Shared\s+)?Function\s+(?<name>[A-Za-z_]\w*)\s*\((?<args>[^)]*)\)\s+As\s+(?<ret>\w+)",
                    RegexOptions.Multiline | RegexOptions.Singleline,
                    TimeSpan.FromSeconds(1)  // timeout recomendado
                );


            foreach (Match m in rx.Matches(vbCode))
            {
                var name = m.Groups["name"].Value.Trim();
                var ret = m.Groups["ret"].Value.Trim();
                var args = (m.Groups["args"].Value ?? "")
                            .Split(',')
                            .Select(a => a.Trim())
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .ToList();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    result.Add(new VbFunctionSig
                    {
                        Name = name,
                        ReturnType = string.IsNullOrWhiteSpace(ret) ? "Decimal" : ret,
                        Parameters = args
                    });
                }
            }

            // Distinct por nombre (por si hay duplicados)
            result = result
                .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            return result;
        }

        // --------- NUEVO: helpers JSON genéricos ---------

        // Extrae de JSON todas las claves booleanas/numéricas como funciones con retorno constante.
        private static Dictionary<string, int> ParseConstantFunctionsFromJson(string json)
        {
            var jo = JObject.Parse(json);
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in jo.Properties())
            {
                var t = prop.Value?.Type ?? JTokenType.Null;
                if (t == JTokenType.Boolean)
                {
                    dict[prop.Name] = (prop.Value != null && (bool)prop.Value) ? 1 : 0;
                }
                else if (t == JTokenType.Integer || t == JTokenType.Float)
                {
                    if (prop.Value != null)
                        dict[prop.Name] = Convert.ToInt32(prop.Value.ToString(), CultureInfo.InvariantCulture);
                }
                // Strings u otros tipos se ignoran para no romper firmas (puedes extender aquí si lo necesitas)
            }

            return dict;
        }

        // Lee una flag int? de un JObject given key (true/false -> 1/0; número -> ese valor; otro -> null)
        private static int? TryParseFlag(JObject jo, string key)
        {
            if (!jo.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var tok) || tok == null)
                return null;

            return tok.Type switch
            {
                JTokenType.Boolean => ((bool)tok) ? 1 : 0,
                JTokenType.Integer or JTokenType.Float => Convert.ToInt32(tok.ToString(), CultureInfo.InvariantCulture),
                _ => (int?)null
            };
        }

        public ErrorDto<object> ReportesInfo(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            // objeto con string logo y nombre empresa
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = "OK",
                Result = new { LOGO_WEB_SITE = "", Nombre = "" }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    resp.Result = connection.Query<object>("SELECT LOGO_WEB_SITE, Nombre FROM SIF_EMPRESA").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
                resp.Result = new { LOGO_WEB_SITE = "https://cdn.prod.website-files.com/6556c20a6abfe6cb4b3b0f09/656e6869ae3ee6a4e179f28b_SystemLogic_LOGO-04-p-500.png", Nombre = "SystemLogic" };
            }
            return resp;
        }

        #endregion

        #region VERSION 1 (Proyecto Angular v1)
        public ErrorDto<object> ReporteRDLC(FrmReporteGlobal data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(data.codEmpresa);
            ErrorDto<object> error = new ErrorDto<object>();

            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    LocalReport report = new LocalReport();
                    string path = Path.Combine(dirRDLC, data.codEmpresa.ToString(), $"{data.nombreReporte}.rdlc");
                    report.EnableExternalImages = true;
                    report.ReportPath = path;

                    // Leer el archivo del reporte principal
                    XDocument doc = XDocument.Load(path);

                    // Lista combinada de datasets (principal + subreportes)
                    var allDatasets = new List<(string ReportName, string DataSetName, string Query)>();

                    // 1. Extraer DataSets del RDLC principal
                    var mainDatasets = doc.Descendants()
                        .Where(x => x.Name.LocalName == "DataSet")
                        .Select(ds => (
                            ReportName: data.nombreReporte ?? string.Empty,
                            DataSetName: ds.Attribute("Name")?.Value ?? string.Empty,
                            Query: ds.Descendants().FirstOrDefault(q => q.Name.LocalName == "CommandText")?.Value ?? string.Empty
                        ))
                        .ToList();

                    allDatasets.AddRange(mainDatasets);

                    // 2. Detectar y cargar subreportes
                    var subreportNames = doc.Descendants()
                        .Where(x => x.Name.LocalName == "Subreport")
                        .Select(x => x.Elements().FirstOrDefault(e => e.Name.LocalName == "ReportName")?.Value)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList();

                    foreach (var subreportName in subreportNames)
                    {
                        string subPath = Path.Combine(dirRDLC, data.codEmpresa.ToString(), $"{subreportName}.rdlc");

                        if (File.Exists(subPath))
                        {
                            XDocument subDoc = XDocument.Load(subPath);

                            var subDatasets = subDoc.Descendants()
                                .Where(x => x.Name.LocalName == "DataSet")
                                .Select(ds => (
                                    ReportName: subreportName ?? string.Empty,
                                    DataSetName: ds.Attribute("Name")?.Value ?? string.Empty,
                                    Query: ds.Descendants().FirstOrDefault(q => q.Name.LocalName == "CommandText")?.Value ?? string.Empty
                                ))
                                .ToList();

                            allDatasets.AddRange(subDatasets);
                        }
                    }

                    // 3. Preparar parámetros
                    var reporteParametros = new List<ReportParameter>();
                    JObject? jObject = null;

                    if (data.parametros != null)
                    {
                        jObject = JObject.Parse(data.parametros);

                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Name != "urlLogo")
                            {
                                reporteParametros.Add(new ReportParameter(prop.Name, prop.Value?.ToString() ?? ""));
                            }

                        }

                        if (data.parametros.Contains("conString"))
                        {
                            reporteParametros.Add(new ReportParameter("conString", stringConn));
                        }

                        if (data.parametros.Contains("urlLogo"))
                        {
                            var queryLogo = "SELECT LOGO_WEB_SITE FROM SIF_EMPRESA";
                            var logo = connection.Query<string>(queryLogo).FirstOrDefault();
                            reporteParametros.Add(new ReportParameter("urlLogo", logo));
                        }

                        report.SetParameters(reporteParametros);
                    }

                    // 4. Ejecutar cada consulta y asociarla al ReportDataSource
                    foreach (var ds in allDatasets)
                    {
                        string query = ds.Query;

                        if (jObject != null && !string.IsNullOrEmpty(query))
                        {
                            foreach (var prop in jObject.Properties())
                            {
                                string nombre = prop.Name;
                                string valor = prop.Value?.ToString() ?? "";
                                query = query.Replace($"@{nombre}", string.IsNullOrWhiteSpace(valor) ? "NULL" : $"'{valor}'");
                            }
                        }

                        var tabla = connection.Query(query).ToList();

                        if (data.cod_reporte == "P")
                        {
                            report.DataSources.Add(new ReportDataSource(ds.DataSetName, tabla));
                        }
                        else
                        {
                            error.Result = JsonConvert.SerializeObject(tabla, Formatting.Indented);
                            return error;
                        }
                    }

                    // 5. Renderizar reporte
                    if (data.cod_reporte == "P")
                    {
                        byte[] bytes = report.Render("PDF");
                        error.Result = Convert.ToBase64String(bytes);
                    }

                    error.Code = 0; // OK
                }
            }
            catch (Exception ex)
            {
                error.Code = 1;
                error.Description = ex.Message;
            }

            return error;
        }
        #endregion

    }
}