namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    /// <summary>
    /// Reutiliza la configuración inmutable del tipo de gestión durante una consulta.
    /// </summary>
    public sealed class FrmTesEmisionDocumentosTipoGestionCache
    {
        private readonly Func<int, string, string> _resolver;
        private readonly Dictionary<(int Banco, string Tipo), string> _valores = new();

        public FrmTesEmisionDocumentosTipoGestionCache(Func<int, string, string> resolver)
        {
            ArgumentNullException.ThrowIfNull(resolver);
            _resolver = resolver;
        }

        /// <summary>
        /// Obtiene el tipo de gestión una sola vez por combinación de banco y documento.
        /// </summary>
        public string Resolver(int banco, string tipo)
        {
            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            var clave = (banco, tipoNormalizado);

            if (_valores.TryGetValue(clave, out var valor))
            {
                return valor;
            }

            valor = _resolver(banco, tipoNormalizado);
            _valores.Add(clave, valor);
            return valor;
        }
    }
}
