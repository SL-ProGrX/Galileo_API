using Galileo.DataBaseTier.ProGrX_Reportes;
using System.Xml.Linq;

namespace Galileo.DataBaseTier
{
    public sealed class RdlcMetaReader : IRdlcMetaReader
    {
        public (List<RdlcDataSetMeta> dataSets, List<string> subreportNames) ReadRdlcMeta(string rdlcPath)
        {
            var xdoc = XDocument.Load(rdlcPath);
            var ns = xdoc.Root!.GetDefaultNamespace();

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
                var name = (string?)ds.Attribute("Name");
                var meta = dataSets.FirstOrDefault(m => m.DataSetName == name);
                var qps = ds.Descendants(ns + "QueryParameters").FirstOrDefault();
                if (qps != null && meta != null)
                {
                    foreach (var qp in qps.Elements(ns + "QueryParameter"))
                    {
                        meta.QueryParams.Add((
                            qp.Attribute("Name")?.Value ?? string.Empty,
                            qp.Element(ns + "Value")?.Value
                        ));
                    }
                }
            }

            var subreports = xdoc.Descendants(ns + "Subreport")
                .Select(s => s.Element(ns + "ReportName")?.Value)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!)  // ← Esto reemplaza la necesidad del .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (dataSets, subreports);
        }
    }
}

