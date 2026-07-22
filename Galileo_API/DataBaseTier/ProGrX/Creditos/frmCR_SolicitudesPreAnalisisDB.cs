using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSolicitudesPreAnalisisDb
    {
        private const string EstadoAsociado = "S";

        private const string MsgSolicitudRangoRequerido =
            "Debe indicar la solicitud inicial y final.";
        private const string MsgSolicitudRangoInvalido =
            "La solicitud inicial no puede ser mayor que la final.";
        private const string MsgPantallaError =
            "No fue posible cargar la informaci&oacute;n inicial.";
        private const string MsgConsultaError =
            "No fue posible consultar la informaci&oacute;n del pre an&aacute;lisis.";

        private const string SqlPantalla = @"
            select
                id_comite,
                rtrim(descripcion) as descripcion,
                isnull(acta, 0) as acta
            from comites
            order by descripcion;";

        private const string SqlConsultaSolicitud = @"
            select
                R.id_solicitud,
                rtrim(R.cedula) as cedula,
                isnull(dbo.fxCRDClasificacion(R.cedula, getdate()), '') as categoria,
                case
                    when isnull(S.estadoactual, '') = 'S' then S.fechaingreso
                    else getdate()
                end as fecha_ingreso,
                isnull(S.estadoactual, '') as estadoactual
            from reg_creditos R
            left join socios S on R.cedula = S.cedula
            where R.estadosol = 'R'
              and R.id_solicitud between @SolicitudDesde and @SolicitudHasta
            order by R.id_solicitud;";

        private readonly PortalDB _portalDb;

        public FrmCrSolicitudesPreAnalisisDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la informacion inicial del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrSolicitudesPreAnalisisPantallaData> CrSolicitudesPreAnalisis_Pantalla_Obtener(
            int codEmpresa)
        {
            var resp = DbHelper.ExecuteListQuery<CrSolicitudesPreAnalisisComiteDto>(
                _portalDb,
                codEmpresa,
                SqlPantalla);

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? MsgPantallaError,
                    resp.Code.GetValueOrDefault(-1),
                    new CrSolicitudesPreAnalisisPantallaData());
            }

            return DbHelper.CreateOkResponse(new CrSolicitudesPreAnalisisPantallaData
            {
                comites = resp.Result ?? new List<CrSolicitudesPreAnalisisComiteDto>()
            });
        }

        /// <summary>
        /// Obtiene las operaciones para pre analisis por solicitud.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSolicitudesPreAnalisisConsultaData> CrSolicitudesPreAnalisis_Consulta_Obtener(
            int codEmpresa,
            CrSolicitudesPreAnalisisConsultaRequest request)
        {
            request ??= new CrSolicitudesPreAnalisisConsultaRequest();

            var validacion = ValidarConsultaPorSolicitud(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Par&aacute;metros inv&aacute;lidos.",
                    validacion.Code.GetValueOrDefault(-2),
                    new CrSolicitudesPreAnalisisConsultaData());
            }

            var resp = DbHelper.ExecuteListQuery<CrSolicitudesPreAnalisisOperacionQueryDto>(
                _portalDb,
                codEmpresa,
                SqlConsultaSolicitud,
                new
                {
                    SolicitudDesde = request.solicitud_desde,
                    SolicitudHasta = request.solicitud_hasta
                });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? MsgConsultaError,
                    resp.Code.GetValueOrDefault(-1),
                    new CrSolicitudesPreAnalisisConsultaData());
            }

            return DbHelper.CreateOkResponse(new CrSolicitudesPreAnalisisConsultaData
            {
                lista = MapearOperacionesSolicitud(resp.Result)
            });
        }

        private static ErrorDto ValidarConsultaPorSolicitud(
            CrSolicitudesPreAnalisisConsultaRequest request)
        {
            if (!request.solicitud_desde.HasValue || !request.solicitud_hasta.HasValue)
            {
                return DbHelper.ErrorResponse(MsgSolicitudRangoRequerido, -2);
            }

            if (request.solicitud_desde.Value > request.solicitud_hasta.Value)
            {
                return DbHelper.ErrorResponse(MsgSolicitudRangoInvalido, -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static List<CrSolicitudesPreAnalisisOperacionDto> MapearOperacionesSolicitud(
            List<CrSolicitudesPreAnalisisOperacionQueryDto>? items)
        {
            return (items ?? new List<CrSolicitudesPreAnalisisOperacionQueryDto>())
                .Select(item => new CrSolicitudesPreAnalisisOperacionDto
                {
                    id_solicitud = item.id_solicitud,
                    cedula = item.cedula,
                    categoria = item.categoria,
                    membresia = ObtenerMembresia(item.estadoactual, item.fecha_ingreso)
                })
                .ToList();
        }

        private static string ObtenerMembresia(string? estadoActual, DateTime? fechaIngreso)
        {
            if (!string.Equals(
                estadoActual,
                EstadoAsociado,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Esta persona no es Asociado";
            }

            if (!fechaIngreso.HasValue)
            {
                return string.Empty;
            }

            return MCredito.fxMembresia(fechaIngreso.Value);
        }
    }
}