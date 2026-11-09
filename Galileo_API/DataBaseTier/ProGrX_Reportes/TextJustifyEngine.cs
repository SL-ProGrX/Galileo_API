using System.Drawing;
using System.Runtime.Versioning;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    /// <summary>
    /// Justifica texto repartiendo espacios entre palabras, midiendo con la fuente real (GDI+).
    /// El motor de RDLC no soporta justificado, por lo que el texto se entrega ya ajustado.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class TextJustifyEngine : IDisposable
    {
        // Margen de seguridad para que el motor no vuelva a partir las líneas ya ajustadas.
        private const double SafetyFactor = 0.985;

        // Si una línea necesitaría más espacios extra por hueco que esto, se deja sin justificar.
        private const int MaxExtraSpacesPerGap = 3;

        private readonly Bitmap _bitmap = new(1, 1);
        private readonly Graphics _graphics;
        private readonly StringFormat _format = StringFormat.GenericTypographic;

        public TextJustifyEngine()
        {
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.PageUnit = GraphicsUnit.Point;
            _format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
        }

        /// <summary>
        /// Justifica un texto (varios párrafos separados por salto de línea) al ancho de la regla.
        /// </summary>
        /// <param name="text">Texto original.</param>
        /// <param name="rule">Ancho útil y fuente del cuadro de texto.</param>
        /// <returns>Texto con espacios repartidos; la última línea de cada párrafo queda a la izquierda.</returns>
        public string Justify(string text, JustifyRule rule)
        {
            if (string.IsNullOrWhiteSpace(text) || rule.WidthPoints <= 0)
                return text;

            using var font = CreateFont(rule);
            var context = new Measure(font, SpaceWidth(font), rule.WidthPoints * SafetyFactor);
            var paragraphs = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            return string.Join("\r\n", paragraphs.Select(p => JustifyParagraph(p, context)));
        }

        private sealed record Measure(Font Font, float Space, double Available);

        private static Font CreateFont(JustifyRule rule) =>
            new(rule.FontFamily, rule.FontSizePoints, rule.Bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);

        private string JustifyParagraph(string paragraph, Measure m)
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
                return paragraph;

            var indent = paragraph.Length - paragraph.TrimStart(' ').Length;
            var lines = BreakLines(words, m, indent);
            var output = new List<string>(lines.Count);

            for (var i = 0; i < lines.Count; i++)
            {
                var esUltima = i == lines.Count - 1;
                var prefijo = i == 0 ? new string(' ', indent) : string.Empty;
                var ancho = m.Available - (i == 0 ? indent * m.Space : 0);
                output.Add(prefijo + (esUltima ? string.Join(' ', lines[i]) : JoinJustified(lines[i], ancho, m)));
            }

            return string.Join("\r\n", output);
        }

        private List<List<string>> BreakLines(string[] words, Measure m, int indent)
        {
            var lines = new List<List<string>>();
            var current = new List<string>();
            var width = 0.0;

            foreach (var word in words)
            {
                var wordWidth = WidthOf(word, m.Font);
                var limite = m.Available - (lines.Count == 0 ? indent * m.Space : 0);
                var needed = current.Count == 0 ? wordWidth : width + m.Space + wordWidth;

                if (current.Count > 0 && needed > limite)
                {
                    lines.Add(current);
                    current = new List<string>();
                    needed = wordWidth;
                }

                current.Add(word);
                width = needed;
            }

            lines.Add(current);
            return lines;
        }

        private string JoinJustified(List<string> words, double available, Measure m)
        {
            if (words.Count < 2)
                return string.Join(' ', words);

            var natural = words.Sum(w => WidthOf(w, m.Font)) + (words.Count - 1) * m.Space;
            var extraSpaces = (int)Math.Floor((available - natural) / m.Space);
            var gaps = words.Count - 1;

            if (extraSpaces < 1 || extraSpaces / gaps > MaxExtraSpacesPerGap)
                return string.Join(' ', words);

            var baseExtra = extraSpaces / gaps;
            var remainder = extraSpaces % gaps;
            var builder = new System.Text.StringBuilder(words[0]);

            for (var gap = 0; gap < gaps; gap++)
            {
                var extra = baseExtra + ((gap + 1) * remainder / gaps) - (gap * remainder / gaps);
                builder.Append(' ', 1 + extra).Append(words[gap + 1]);
            }

            return builder.ToString();
        }

        private double WidthOf(string text, Font font) =>
            _graphics.MeasureString(text, font, new SizeF(100000, 1000), _format).Width;

        private float SpaceWidth(Font font) =>
            (float)(WidthOf("a a", font) - WidthOf("aa", font));

        public void Dispose()
        {
            _graphics.Dispose();
            _bitmap.Dispose();
        }
    }
}
