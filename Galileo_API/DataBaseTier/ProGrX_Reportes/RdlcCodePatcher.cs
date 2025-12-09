using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Galileo.DataBaseTier
{
    public sealed class RdlcCodePatcher  : IRdlcCodePatcher
    {
        public MemoryStream PatchReportCode(string rdlcPath, string? codeSection)
        {
            var xdoc = XDocument.Load(rdlcPath);
            var ns = xdoc.Root!.GetDefaultNamespace();
            var codeNode = xdoc.Descendants(ns + "Code").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(codeSection))
                return SaveToStream(xdoc);

            if (TryParseConstantFunctionsFromJson(codeSection, out var constFuncs))
            {
                var codeText = codeNode?.Value ?? string.Empty;
                foreach (var kv in constFuncs)
                    codeText = UpsertFunctionReturn(codeText, kv.Key, kv.Value);

                if (codeNode == null)
                    xdoc.Root!.Add(new XElement(ns + "Code", codeText));
                else
                    codeNode.Value = codeText;

                return SaveToStream(xdoc);
            }

            // VB crudo
            if (codeNode == null)
                xdoc.Root!.Add(new XElement(ns + "Code", codeSection));
            else
                codeNode.Value = codeSection;

            return SaveToStream(xdoc);
        }

        public (int? fxDetConst, int? fxRefConst) ParseFxConstants(string? codeSection)
        {
            if (string.IsNullOrWhiteSpace(codeSection)) return (null, null);
            try
            {
                var jo = JObject.Parse(codeSection);
                return (TryParseFlag(jo, "fxImprimeDetalle"), TryParseFlag(jo, "fxImprimeRef"));
            }
            catch
            {
                return (GetCodeFunctionConstantReturnFromText(codeSection, "fxImprimeDetalle"),
                        GetCodeFunctionConstantReturnFromText(codeSection, "fxImprimeRef"));
            }
        }

        private static MemoryStream SaveToStream(XDocument xdoc)
        {
            var ms = new MemoryStream();
            xdoc.Save(ms);
            ms.Position = 0;
            return ms;
        }

        private static string UpsertFunctionReturn(string codeText, string funcName, int ret)
        {
            var funcBlock = new Regex($@"(?is)Public\s+Function\s+{Regex.Escape(funcName)}\s*\(\s*\)\s+As\s+\w+.*?End\s+Function");

            if (funcBlock.IsMatch(codeText))
            {
                codeText = funcBlock.Replace(codeText, m =>
                {
                    var body = m.Value;
                    var withReturn = Regex.Replace(body, @"(?im)^\s*Return\s+.*$", $"    Return {ret}");
                    if (!Regex.IsMatch(withReturn, @"(?im)^\s*Return\s+", RegexOptions.Multiline))
                        withReturn = Regex.Replace(withReturn, @"(?i)End\s+Function", $"    Return {ret}\nEnd Function");
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

        private static bool TryParseConstantFunctionsFromJson(string json, out Dictionary<string, int> dict)
        {
            dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var jo = JObject.Parse(json);
                foreach (var prop in jo.Properties())
                {
                    var t = prop.Value?.Type ?? JTokenType.Null;
                    if (t == JTokenType.Boolean)
                        dict[prop.Name] = ((bool)prop.Value!) ? 1 : 0;
                    else if (t == JTokenType.Integer || t == JTokenType.Float)
                        dict[prop.Name] = Convert.ToInt32(prop.Value!.ToString(), CultureInfo.InvariantCulture);
                }
                return true;
            }
            catch { return false; }
        }

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

        private static int? GetCodeFunctionConstantReturnFromText(string codeText, string funcName)
        {
            var pattern = $@"Public\s+Function\s+{Regex.Escape(funcName)}\s*\(\s*\)\s+As\s+\w+.*?Return\s+(?<ret>-?\d+)\s*[\r\n]+End\s+Function";
            var m = Regex.Match(codeText ?? string.Empty, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            return int.TryParse(m.Groups["ret"].Value, out var val) ? val : null;
        }
    }
}

