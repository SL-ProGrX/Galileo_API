using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    /// <summary>
    /// Regla de justificado de un cuadro de texto de un RDLC: campo, ancho útil y fuente.
    /// </summary>
    public sealed record JustifyRule(
        string DataSetName,
        string FieldName,
        double WidthPoints,
        string FontFamily,
        float FontSizePoints,
        bool Bold);

    /// <summary>
    /// Lee un RDLC y detecta los cuadros de texto con alineación Default/General
    /// cuyo contenido es un campo simple del dataset.
    /// </summary>
    public static class RdlcJustifyRuleReader
    {
        private const double DefaultPaddingPoints = 2;
        private const double PointsPerInch = 72;
        private const float DefaultFontSize = 10f;
        private const string DefaultFontFamily = "Arial";

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        private static readonly Regex FieldExpression = new(
            @"^=\s*(?:(?:First|Last)\s*\(\s*)?Fields!(?<f>\w+)\.Value(?:\s*,\s*""(?<d>[^""]+)"")?\s*\)?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            RegexTimeout);

        private static readonly Regex SizeExpression = new(
            @"^\s*(?<n>-?\d+(?:\.\d+)?)\s*(?<u>in|pt|cm|mm)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            RegexTimeout);

        /// <summary>
        /// Obtiene las reglas de justificado de un RDLC (una por campo y dataset).
        /// </summary>
        /// <param name="rdlcPath">Ruta del archivo RDLC.</param>
        /// <returns>Reglas detectadas; lista vacía si el archivo no existe o no aplica.</returns>
        public static List<JustifyRule> Read(string rdlcPath)
        {
            var rules = new List<JustifyRule>();
            if (string.IsNullOrWhiteSpace(rdlcPath) || !File.Exists(rdlcPath))
                return rules;

            var doc = XDocument.Load(rdlcPath);
            if (doc.Root is null)
                return rules;

            var ns = doc.Root.GetDefaultNamespace();
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var textbox in doc.Descendants(ns + "Textbox"))
            {
                var rule = BuildRule(textbox, ns);
                if (rule is not null && vistos.Add($"{rule.DataSetName}|{rule.FieldName}"))
                    rules.Add(rule);
            }

            return rules;
        }

        private static JustifyRule? BuildRule(XElement textbox, XNamespace ns)
        {
            var paragraphs = textbox.Element(ns + "Paragraphs")?.Elements(ns + "Paragraph").ToList();
            if (paragraphs is null || paragraphs.Count != 1 || !EsAlineacionGeneral(paragraphs[0], ns))
                return null;

            var runs = paragraphs[0].Element(ns + "TextRuns")?.Elements(ns + "TextRun").ToList();
            if (runs is null || runs.Count != 1)
                return null;

            var match = MatchField(runs[0], ns);
            var width = ParsePoints(textbox.Element(ns + "Width")?.Value);
            var dataSet = ResolveDataSet(textbox, match, ns);
            if (match is null || width is null || dataSet is null)
                return null;

            var padding = ReadPadding(textbox, ns);
            var style = runs[0].Element(ns + "Style");

            return new JustifyRule(
                dataSet,
                match.Groups["f"].Value,
                Math.Max(0, width.Value - padding),
                style?.Element(ns + "FontFamily")?.Value ?? DefaultFontFamily,
                ParseFontSize(style?.Element(ns + "FontSize")?.Value),
                EsNegrita(style?.Element(ns + "FontWeight")?.Value));
        }

        private static bool EsAlineacionGeneral(XElement paragraph, XNamespace ns)
        {
            var align = paragraph.Element(ns + "Style")?.Element(ns + "TextAlign")?.Value;
            return string.IsNullOrWhiteSpace(align)
                || align.Equals("Default", StringComparison.OrdinalIgnoreCase)
                || align.Equals("General", StringComparison.OrdinalIgnoreCase);
        }

        private static Match? MatchField(XElement run, XNamespace ns)
        {
            var value = run.Element(ns + "Value")?.Value?.Trim();
            if (string.IsNullOrEmpty(value))
                return null;

            var match = FieldExpression.Match(value);
            return match.Success ? match : null;
        }

        private static string? ResolveDataSet(XElement textbox, Match? match, XNamespace ns)
        {
            var explicito = match?.Groups["d"].Value;
            if (!string.IsNullOrWhiteSpace(explicito))
                return explicito;

            return textbox.Ancestors(ns + "Tablix")
                .Select(t => t.Element(ns + "DataSetName")?.Value)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
        }

        private static double ReadPadding(XElement textbox, XNamespace ns)
        {
            var style = textbox.Element(ns + "Style");
            var left = ParsePoints(style?.Element(ns + "PaddingLeft")?.Value) ?? DefaultPaddingPoints;
            var right = ParsePoints(style?.Element(ns + "PaddingRight")?.Value) ?? DefaultPaddingPoints;
            return left + right;
        }

        private static float ParseFontSize(string? value)
        {
            var points = ParsePoints(value);
            return points is > 0 ? (float)points.Value : DefaultFontSize;
        }

        private static bool EsNegrita(string? weight)
        {
            if (string.IsNullOrWhiteSpace(weight))
                return false;

            if (int.TryParse(weight, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero))
                return numero >= 600;

            return weight.Equals("Bold", StringComparison.OrdinalIgnoreCase)
                || weight.Equals("Bolder", StringComparison.OrdinalIgnoreCase);
        }

        private static double? ParsePoints(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var match = SizeExpression.Match(value);
            if (!match.Success)
                return null;

            var number = double.Parse(match.Groups["n"].Value, CultureInfo.InvariantCulture);
            return match.Groups["u"].Value.ToLowerInvariant() switch
            {
                "in" => number * PointsPerInch,
                "cm" => number / 2.54 * PointsPerInch,
                "mm" => number / 25.4 * PointsPerInch,
                _ => number
            };
        }
    }
}
