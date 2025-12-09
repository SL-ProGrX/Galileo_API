using Galileo_API.Controllers.WFCSinpe;

namespace Galileo_API.DataBaseTier
{
    public class VerificadorCoreFactory
    {
        private readonly IConfiguration _config;
        private readonly MKindoServiceDb _sinpe;

        public VerificadorCoreFactory(IConfiguration config)
        {
            _config = config;
            _sinpe = new MKindoServiceDb(config);
        }

        public IWfcSinpe CrearServicio(int CodEmpresa, string usuario)
        {
            var nombreTipo = _sinpe.GetUriEmpresa(CodEmpresa, usuario).Result.ServiciosSinpe;

            if (string.IsNullOrWhiteSpace(nombreTipo))
                throw new NotSupportedException($"No se encontró configuración para la empresa {CodEmpresa}");

            // Busca el tipo (debe incluir el namespace completo y estar cargado en el assembly actual)
            var tipo = Type.GetType(nombreTipo);

            if (tipo == null)
                throw new InvalidOperationException($"No se encontró el tipo '{nombreTipo}' en el contexto actual.");

            // Crea la instancia pasando _config al constructor
            var instancia = Activator.CreateInstance(tipo, _config);

            // Valida que realmente implemente la interfaz
            if (instancia is not IWfcSinpe servicio)
                throw new InvalidCastException($"El tipo '{nombreTipo}' no implementa IWFCSinpe.");

            return servicio;

        }
    }
}
