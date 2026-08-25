using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmConTransitoriasBl
    {
        private readonly FrmConTransitoriasDb _db;

        public FrmConTransitoriasBl(IConfiguration config)
        {
            _db = new FrmConTransitoriasDb(config);
        }

        public ErrorDto<ConTransitoriasInicializaData>
            Conciliacion_ConTransitorias_Inicializar(int codEmpresa)
        {
            return _db.Conciliacion_ConTransitorias_Inicializar(codEmpresa);
        }

        public ErrorDto<List<ConTransitoriasData>>
            Conciliacion_ConTransitorias_Consultar(
                int codEmpresa,
                string request)
        {
            ConTransitoriasConsultaRequest? filtros =
                Conciliacion_ConTransitorias_Request_Deserializar(request);

            return _db.Conciliacion_ConTransitorias_Consultar(
                codEmpresa,
                filtros);
        }

        private static ConTransitoriasConsultaRequest?
            Conciliacion_ConTransitorias_Request_Deserializar(string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<
                    ConTransitoriasConsultaRequest>(request);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
