namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    /// <summary>
    /// Justifica los textos de los reportes RDLC habilitados por configuración
    /// (sección <c>Reportes:JustificarTexto</c>), sin modificar los archivos .rdl.
    /// </summary>
    public sealed class RdlcTextJustifier : IRdlcTextJustifier
    {
        private readonly HashSet<string> _habilitados;

        /// <summary>
        /// Inicializa el justificador con los nombres de reportes habilitados.
        /// </summary>
        /// <param name="reportesHabilitados">Nombres de reportes (con o sin extensión).</param>
        public RdlcTextJustifier(IEnumerable<string>? reportesHabilitados)
        {
            _habilitados = new HashSet<string>(
                (reportesHabilitados ?? []).Select(Normalizar),
                StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<JustifyRule> ReadRules(string reportName, string rdlcPath)
        {
            if (!OperatingSystem.IsWindows() || !_habilitados.Contains(Normalizar(reportName)))
                return [];

            return RdlcJustifyRuleReader.Read(rdlcPath);
        }

        public IEnumerable<object> Apply(
            string dataSetName,
            IReadOnlyList<JustifyRule> rules,
            IEnumerable<object> rows)
        {
            if (!OperatingSystem.IsWindows())
                return rows;

            var propias = rules
                .Where(r => r.DataSetName.Equals(dataSetName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (propias.Count == 0)
                return rows;

            var lista = rows as IList<object> ?? rows.ToList();
            using var engine = new TextJustifyEngine();

            foreach (var row in lista.OfType<IDictionary<string, object>>())
                JustificarFila(row, propias, engine);

            return lista;
        }

        private static void JustificarFila(
            IDictionary<string, object> row,
            List<JustifyRule> rules,
            TextJustifyEngine engine)
        {
            foreach (var rule in rules)
            {
                var key = row.Keys.FirstOrDefault(k => k.Equals(rule.FieldName, StringComparison.OrdinalIgnoreCase));
                if (key is not null && row[key] is string texto && !string.IsNullOrWhiteSpace(texto))
                    row[key] = engine.Justify(texto, rule);
            }
        }

        private static string Normalizar(string nombre)
        {
            var limpio = (nombre ?? string.Empty).Trim();
            foreach (var ext in new[] { ".rdlc", ".rdl" })
            {
                if (limpio.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return limpio[..^ext.Length];
            }

            return limpio;
        }
    }
}
