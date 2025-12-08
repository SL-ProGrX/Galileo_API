using Microsoft.Reporting.NETCore;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Galileo.DataBaseTier.ProGrX_Reportes;
using Galileo.Models;

namespace Galileo.DataBaseTier
{
    public interface IRdlcPathResolver
    {
        string GetBasePath(int codEmpresa, string? folder, string dirRdlc);
        string? ResolveReportPath(string basePath, string reportNameOrRelative);
    }

    public interface IRdlcMetaReader
    {
        (List<RdlcDataSetMeta> dataSets, List<string> subreportNames) ReadRdlcMeta(string rdlcPath);
    }

    public interface IRdlcCodePatcher
    {
        MemoryStream PatchReportCode(string rdlcPath, string? codeSection);
        (int? fxDetConst, int? fxRefConst) ParseFxConstants(string? codeSection);
    }

    public interface IRdlcExecutor
    {
        bool TryExecDataSet(SqlConnection connection,
                            RdlcDataSetMeta ds,
                            IDictionary<string, string> ctx,
                            JObject? jsonParams,
                            bool allowFiltrosReplacement,
                            out IEnumerable<object> rows,
                            out string? error);
    }

    public interface IReportParameterBuilder
    {
        (List<ReportParameter> reportParams, Dictionary<string, string> ctx, JObject? jsonParams)
            Build(FrmReporteGlobal data, SqlConnection connection, string connString);
    }

    public interface ISubreportCoordinator
    {
        Dictionary<string, List<RdlcDataSetMeta>> LoadSubreports(LocalReport report, string basePath, IEnumerable<string> subreportNames);
        Dictionary<string, Dictionary<string, string>> BuildAutoAliasMap(string parentRdlcPath, string basePath);
        void ConfigureSubreportProcessing(LocalReport report,
                                          IReadOnlyDictionary<string, List<RdlcDataSetMeta>> subMeta,
                                          SqlConnection connection,
                                          IReadOnlyDictionary<string, Dictionary<string, string>> autoAliases,
                                          IDictionary<string, string> paramDict,
                                          ICollection<string> subErrors,
                                          (int? fxDetConst, int? fxRefConst) fxConstants);
    }
}
