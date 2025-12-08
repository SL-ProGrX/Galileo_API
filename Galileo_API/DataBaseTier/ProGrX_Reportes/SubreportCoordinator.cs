using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json.Linq;
using Galileo.DataBaseTier.ProGrX_Reportes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Coordina el procesamiento de subreportes para reportes RDLC.
    /// </summary>
    public sealed class SubreportCoordinator : ISubreportCoordinator
    {
        private readonly IRdlcMetaReader _meta;
        private readonly IRdlcPathResolver _paths;
        private readonly IRdlcExecutor _executor;

        // Timeout común para las expresiones regulares de esta clase
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        public SubreportCoordinator(IRdlcMetaReader meta, IRdlcPathResolver paths, IRdlcExecutor executor)
        {
            _meta    = meta;
            _paths   = paths;
            _executor = executor;
        }

        // ================================================================
        // =================== CARGA DE SUBREPORTES =======================
        // ================================================================

        public Dictionary<string, List<RdlcDataSetMeta>> LoadSubreports(LocalReport report, string basePath, IEnumerable<string> subreportNames)
        {
            var subMeta = new Dictionary<string, List<RdlcDataSetMeta>>(StringComparer.OrdinalIgnoreCase);

            foreach (var subName in subreportNames)
            {
                var subPath = _paths.ResolveReportPath(basePath, subName);
                if (subPath == null) continue;

                using var fs = File.OpenRead(subPath);
                report.LoadSubreportDefinition(subName, fs);

                var (subDs, _) = _meta.ReadRdlcMeta(subPath);
                subMeta[subName] = subDs;
            }

            return subMeta;
        }

        // ================================================================
        // =================== ALIAS AUTOMÁTICOS ==========================
        // ================================================================

        public Dictionary<string, Dictionary<string, string>> BuildAutoAliasMap(string parentRdlcPath, string basePath)
        {
            var parentMap = ReadParentSubreportParamNames(parentRdlcPath);
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in parentMap)
            {
                var alias = BuildAliasForSubreport(kv.Key, kv.Value, basePath);
                if (alias.Count > 0)
                    result[kv.Key] = alias;
            }

            return result;
        }

        private Dictionary<string, string> BuildAliasForSubreport(string subName, List<string> parentParams, string basePath)
        {
            var alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var childPath = _paths.ResolveReportPath(basePath, subName);
            if (childPath == null) return alias;

            var childDoc = XDocument.Load(childPath);
            var expected = ReadExpectedChildParams(childDoc).ToList();

            if (expected.Count == 1)
                return parentParams.ToDictionary(p => p, _ => expected[0], StringComparer.OrdinalIgnoreCase);

            return MapMultiParamAliases(parentParams, expected);
        }

        private static Dictionary<string, string> MapMultiParamAliases(List<string> parentParams, List<string> expected)
        {
            var alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var expNorm = expected.ToDictionary(Norm, e => e, StringComparer.OrdinalIgnoreCase);

            foreach (var parent in parentParams)
            {
                var np = Norm(parent);
                if (expNorm.TryGetValue(np, out var hit))
                    alias[parent] = hit;
                else
                    MapPartialMatch(alias, parent, np, expected);
            }

            EnsureFallbackMapping(alias, parentParams, expected);
            return alias;
        }

        private static void MapPartialMatch(Dictionary<string, string> alias, string parent, string np, List<string> expected)
        {
            var hit2 = expected.FirstOrDefault(e => Norm(e).EndsWith(np) || np.EndsWith(Norm(e)));
            if (!string.IsNullOrEmpty(hit2))
                alias[parent] = hit2;
        }

        private static void EnsureFallbackMapping(Dictionary<string, string> alias, List<string> parentParams, List<string> expected)
        {
            if (alias.Count < Math.Min(parentParams.Count, expected.Count) && parentParams.Count == expected.Count)
            {
                for (int i = 0; i < parentParams.Count; i++)
                {
                    if (!alias.ContainsKey(parentParams[i]))
                        alias[parentParams[i]] = expected[i];
                }
            }
        }

        // ================================================================
        // ================= CONFIGURACIÓN DE SUBREPORTS ==================
        // ================================================================

        public void ConfigureSubreportProcessing(LocalReport report,
                                                 IReadOnlyDictionary<string, List<RdlcDataSetMeta>> subMeta,
                                                 SqlConnection connection,
                                                 IReadOnlyDictionary<string, Dictionary<string, string>> autoAliases,
                                                 IDictionary<string, string> paramDict,
                                                 ICollection<string> subErrors,
                                                 (int? fxDetConst, int? fxRefConst) fxConstants)
        {
            report.SubreportProcessing += (_, e) =>
            {
                try
                {
                    if (!subMeta.TryGetValue(e.ReportPath, out var datasets))
                        return;

                    if (ShouldSkipSubreport(e.ReportPath, fxConstants))
                    {
                        AddEmptyDataSources(e, datasets);
                        return;
                    }

                    var merged = MergeParentAndSubreportParameters(e.Parameters, paramDict, e.ReportPath, autoAliases);
                    ProcessSubreportDataSets(connection, datasets, e, merged, subErrors);
                }
                catch (Exception exSub)
                {
                    subErrors.Add($"[SUB '{e.ReportPath}'] {exSub.GetType().Name}: {exSub.Message}");
                }
            };
        }

        private static void AddEmptyDataSources(SubreportProcessingEventArgs e, IEnumerable<RdlcDataSetMeta> datasets)
        {
            foreach (var ds in datasets)
                e.DataSources.Add(new ReportDataSource(ds.DataSetName, Enumerable.Empty<object>()));
        }

        private static Dictionary<string, string> MergeParentAndSubreportParameters(IEnumerable<ReportParameterInfo> parameters,
                                                                                   IDictionary<string, string> parentParams,
                                                                                   string subName,
                                                                                   IReadOnlyDictionary<string, Dictionary<string, string>> autoAliases)
        {
            var merged = new Dictionary<string, string>(parentParams, StringComparer.OrdinalIgnoreCase);
            foreach (var p in parameters)
                merged[p.Name] = p.Values?.FirstOrDefault() ?? string.Empty;

            if (autoAliases.TryGetValue(subName, out var aliasMap))
            {
                foreach (var kv in aliasMap)
                {
                    if (!merged.ContainsKey(kv.Value) && merged.TryGetValue(kv.Key, out var v))
                        merged[kv.Value] = v;
                }
            }

            return merged;
        }

        private void ProcessSubreportDataSets(SqlConnection connection,
                                              IEnumerable<RdlcDataSetMeta> datasets,
                                              SubreportProcessingEventArgs e,
                                              IDictionary<string, string> merged,
                                              ICollection<string> subErrors)
        {
            foreach (var ds in datasets)
            {
                if (string.IsNullOrWhiteSpace(ds.CommandText))
                {
                    e.DataSources.Add(new ReportDataSource(ds.DataSetName, Enumerable.Empty<object>()));
                    continue;
                }

                if (!_executor.TryExecDataSet(connection, ds, merged, JObject.FromObject(merged), false, out var rows, out var err))
                {
                    if (!string.IsNullOrWhiteSpace(err))
                        subErrors.Add($"[SUB '{e.ReportPath}' DS '{ds.DataSetName}'] {err}");

                    e.DataSources.Add(new ReportDataSource(ds.DataSetName, Enumerable.Empty<object>()));
                }
                else
                {
                    e.DataSources.Add(new ReportDataSource(ds.DataSetName, rows));
                }
            }
        }

        private static bool ShouldSkipSubreport(string subName, (int? fxDetConst, int? fxRefConst) fx)
        {
            if (fx.fxRefConst == 0 && subName.Contains("ref", StringComparison.OrdinalIgnoreCase))
                return true;

            if (fx.fxDetConst == 0 &&
                (subName.Contains("det", StringComparison.OrdinalIgnoreCase) ||
                 subName.Contains("detalle", StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        // ================================================================
        // ================= MÉTODOS AUXILIARES ===========================
        // ================================================================

        private static Dictionary<string, List<string>> ReadParentSubreportParamNames(string parentRdlcPath)
        {
            var x  = XDocument.Load(parentRdlcPath);
            var ns = x.Root!.GetDefaultNamespace();
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var sr in x.Descendants(ns + "Subreport"))
            {
                var reportName = sr.Element(ns + "ReportName")?.Value;
                if (string.IsNullOrWhiteSpace(reportName)) continue;

                var list = sr.Element(ns + "Parameters")?
                            .Elements(ns + "Parameter")
                            .Select(p => p.Attribute("Name")?.Value)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Cast<string>()
                            .ToList() ?? new List<string>();

                map[reportName] = list;
            }

            return map;
        }

        private static HashSet<string> ReadExpectedChildParams(XDocument childDoc)
        {
            var ns = childDoc.Root!.GetDefaultNamespace();
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

                var trimmed = valExpr.Trim();

                // Antes: Regex.Match(trimmed, @"^=Parameters!(?<p>\w+)\.Value$", RegexOptions.IgnoreCase);
                var m = Regex.Match(
                    trimmed,
                    @"^=Parameters!(?<p>\w+)\.Value$",
                    RegexOptions.IgnoreCase,
                    RegexTimeout);

                if (m.Success) expected.Add(m.Groups["p"].Value);
            }

            return expected;
        }

        private static string Norm(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var last = s.Split(new[] { '.', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? s;
            var only = new string(last.Where(char.IsLetterOrDigit).ToArray());
            return only.ToUpperInvariant();
        }
    }
}