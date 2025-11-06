using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Autorizacion_ComprasDB
    {
        private readonly IConfiguration _config;
        private readonly frmCpr_SolicitudDB _DBSolicitud;

        public frmCpr_Autorizacion_ComprasDB(IConfiguration config)
        {
            _config = config;
            _DBSolicitud = new frmCpr_SolicitudDB(config);
        }


        public ErrorDto<List<CprSolicitudAutoriza>> SolicitudAutorizacion_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            CprSolicitudFiltros filtros = JsonConvert.DeserializeObject<CprSolicitudFiltros>(filtroString);

            var response = new ErrorDto<List<CprSolicitudAutoriza>>();
            response.Code = 0;

            try
            {
                string where = "";
                if (filtros.fecha != null)
                {
                    // Convertir la cadena ISO a DateTimeOffset
                    DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fechaInico);
                    string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                    DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fechaCorte);
                    string fechacorte = fecha_corte.ToString("yyyy-MM-dd");

                    switch (filtros.fecha)
                    {
                        case "A":
                            where = $" AND REGISTRO_FECHA between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                            break;
                        case "N":
                            where = $"";
                            break;
                        default:
                            break;
                    }
                }

                where += $" And  COD_UNIDAD = '{filtros.cod_unidad}' ";




                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT CPR_ID, ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO, MONTO, i_presupuestado,
                                    DETALLE, COD_UNIDAD  FROM CPR_SOLICITUD WHERE ESTADO IN ('P') {where} ";

                    response.Result = connection.Query<CprSolicitudAutoriza>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }

        public ErrorDto AutorizaSolicitudes(int CodCliente, string solicitudes, string usuario)
        {
            ErrorDto info = new()
            {
                Code = 0
            };
            List<CprSolicitudAutoriza> solicitudesData = new List<CprSolicitudAutoriza>();
            solicitudesData = JsonConvert.DeserializeObject<List<CprSolicitudAutoriza>>(solicitudes);

            try
            {
                foreach (CprSolicitudAutoriza item in solicitudesData)
                {
                    info = _DBSolicitud.AutorizaSolicitud(CodCliente, item.cpr_id, usuario);
                    if (info.Code == -1)
                    {
                        return info;
                    }
                }
                info.Description = "Se autorizaron las solicitudes de forma correcta!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDto RechazaSolicitudes(int CodCliente, string solicitudes, string justificacion, string usuario)
        {
            ErrorDto info = new()
            {
                Code = 0
            };
            List<CprSolicitudAutoriza> solicitudesData = new List<CprSolicitudAutoriza>();
            solicitudesData = JsonConvert.DeserializeObject<List<CprSolicitudAutoriza>>(solicitudes);

            try
            {
                foreach (CprSolicitudAutoriza item in solicitudesData)
                {
                    info = _DBSolicitud.DeniegaSolicitud(CodCliente, item.cpr_id, usuario, justificacion);
                    if (info.Code == -1)
                    {
                        return info;
                    }
                }
                info.Description = "Se rechazaron las solicitudes de forma correcta!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

    }
}