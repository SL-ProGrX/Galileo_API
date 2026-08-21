using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmRastreoMovDocBl
    {
        private readonly FrmRastreoMovDocDb _db;

        public FrmRastreoMovDocBl(IConfiguration config)
        {
            _db = new FrmRastreoMovDocDb(config);
        }

        public ErrorDto<RastreoMovDocInicializaData>
            Conciliacion_RastreoMovDoc_Inicializar(
                int codEmpresa)
        {
            return _db
                .Conciliacion_RastreoMovDoc_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<List<RastreoMovDocResumenData>>
            Conciliacion_RastreoMovDoc_Resumen_Obtener(
                int codEmpresa,
                string request)
        {
            RastreoMovDocConsultaRequest? filtros =
                Conciliacion_RastreoMovDoc_Request_Deserializar(
                    request);

            return _db
                .Conciliacion_RastreoMovDoc_Resumen_Obtener(
                    codEmpresa,
                    filtros);
        }

        public ErrorDto<List<RastreoMovDocDetalleData>>
            Conciliacion_RastreoMovDoc_Detalle_Obtener(
                int codEmpresa,
                string request)
        {
            RastreoMovDocConsultaRequest? filtros =
                Conciliacion_RastreoMovDoc_Request_Deserializar(
                    request);

            return _db
                .Conciliacion_RastreoMovDoc_Detalle_Obtener(
                    codEmpresa,
                    filtros);
        }

        private static RastreoMovDocConsultaRequest?
            Conciliacion_RastreoMovDoc_Request_Deserializar(
                string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<
                    RastreoMovDocConsultaRequest>(request);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}