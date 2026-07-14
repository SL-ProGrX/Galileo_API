using System.Globalization;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprAutorizacionComprasDB
    {
        private readonly FrmCprSolicitudDB _dbSolicitud;
        private readonly PortalDB _portalDB;

        public FrmCprAutorizacionComprasDB(IConfiguration config)
        {
            _dbSolicitud = new FrmCprSolicitudDB(config);
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<List<CprSolicitudAutoriza>> SolicitudAutorizacion_Obtener(int codCliente, string? filtroString)
        {
            // 1) Parse seguro del filtro
            CprSolicitudFiltros filtros;
            try
            {
                filtros = !string.IsNullOrWhiteSpace(filtroString)
                    ? (JsonConvert.DeserializeObject<CprSolicitudFiltros>(filtroString) ?? new CprSolicitudFiltros())
                    : new CprSolicitudFiltros();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CprSolicitudAutoriza>>($"Filtro inválido: {ex.Message}");
            }

            // 2) Construcción segura del SQL + parámetros
            var sql = new StringBuilder(@"
                                    SELECT
                                        CPR_ID,
                                        ESTADO,
                                        REGISTRO_FECHA,
                                        REGISTRO_USUARIO,
                                        MONTO,
                                        i_presupuestado,
                                        DETALLE,
                                        COD_UNIDAD
                                    FROM CPR_SOLICITUD
                                    WHERE ESTADO IN ('P')
                                    ");

            var p = new DynamicParameters();

            // Fecha (solo acepta "A" o "N")
            if (!string.IsNullOrWhiteSpace(filtros.fecha))
            {
                switch (filtros.fecha)
                {
                    case "A":
                    {
                            string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(Convert.ToDateTime(filtros.fechaInico!), "yyyy-MM-dd" + " 00:00:00") ?? "";
                            string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(Convert.ToDateTime(filtros.fechaCorte!), "yyyy-MM-dd" + " 23:59:59") ?? "";

                       if (string.IsNullOrEmpty(fechaIni))
                            return DbHelper.CreateErrorResponse<List<CprSolicitudAutoriza>>("fechaInico inválida o vacía.");

                        if (string.IsNullOrEmpty(fechaFin))
                            return DbHelper.CreateErrorResponse<List<CprSolicitudAutoriza>>("fechaCorte inválida o vacía.");

                 
                        sql.AppendLine("  AND REGISTRO_FECHA BETWEEN @Desde AND @Hasta");
                        p.Add("@Desde", fechaIni);
                        p.Add("@Hasta", fechaFin);
                        break;
                    }
                    case "N":
                        // sin filtro de fecha
                        break;

                    default:
                        return DbHelper.CreateErrorResponse<List<CprSolicitudAutoriza>>("El filtro 'fecha' solo permite 'A' o 'N'.");
                }
            }

            // COD_UNIDAD (parametrizado)
            if (!string.IsNullOrWhiteSpace(filtros.cod_unidad))
            {
                sql.AppendLine("  AND COD_UNIDAD = @CodUnidad");
                p.Add("@CodUnidad", filtros.cod_unidad);
            }

            sql.AppendLine("ORDER BY REGISTRO_FECHA DESC;");

            // 3) Ejecutar con tu helper (sin concatenar valores del usuario)
            return DbHelper.ExecuteListQuery<CprSolicitudAutoriza>(
                _portalDB,
                codCliente,
                sql.ToString(),
                p
            );
        }

        public ErrorDto AutorizaSolicitudes(int codCliente, string solicitudes, string usuario)
        {
            List<CprSolicitudAutoriza> solicitudesData;
            try
            {
                solicitudesData = JsonConvert.DeserializeObject<List<CprSolicitudAutoriza>>(solicitudes) ?? new();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"JSON de solicitudes inválido: {ex.Message}");
            }

            try
            {
                foreach (var item in solicitudesData)
                {
                    var info = _dbSolicitud.AutorizaSolicitud(codCliente, item.cpr_id, usuario);
                    if (info.Code == -1)
                        return info;
                }

                return DbHelper.OkResponse("Se autorizaron las solicitudes de forma correcta!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto RechazaSolicitudes(int codCliente, string solicitudes, string justificacion, string usuario)
        {
            List<CprSolicitudAutoriza> solicitudesData;
            try
            {
                solicitudesData = JsonConvert.DeserializeObject<List<CprSolicitudAutoriza>>(solicitudes) ?? new();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"JSON de solicitudes inválido: {ex.Message}");
            }

            try
            {
                foreach (var item in solicitudesData)
                {
                    var info = _dbSolicitud.DeniegaSolicitud(codCliente, item.cpr_id, usuario, justificacion);
                    if (info.Code == -1)
                        return info;
                }

                return DbHelper.OkResponse("Se rechazaron las solicitudes de forma correcta!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool TryParseIsoToDateTime(string? value, out DateTime dt)
        {
            dt = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Soporta ISO (con o sin zona), sin depender del culture del server
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
            {
                dt = dto.UtcDateTime;
                return true;
            }

            return false;
        }
    }


}