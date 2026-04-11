using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCONotificaAsociadoAportesAtrasadosDB
    {
        private readonly PortalDB _portalDB;

        public FrmCONotificaAsociadoAportesAtrasadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de asociados con aportes atrasados para notificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(
            int CodEmpresa,
            string? cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedulaFiltro = (cedula ?? string.Empty).Trim();

                var lista = conn.Query<CoNotificaAsociadoAportesAtrasadosData>(
                    "spCBR_Notifica_SociosAportesAtrasados_Consulta",
                    new
                    {
                        Cedula = cedulaFiltro
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(new CoNotificaAsociadoAportesAtrasadosListaResult
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CoNotificaAsociadoAportesAtrasadosListaResult
                    {
                        total = 0,
                        lista = new List<CoNotificaAsociadoAportesAtrasadosData>()
                    });
            }
        }

        /// <summary>
        /// Exporta la lista de asociados con aportes atrasados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export(
            int CodEmpresa,
            string? cedula)
        {
            return CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(CodEmpresa, cedula);
        }

        /// <summary>
        /// Envía notificaciones a los asociados indicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CO_Notifica_Asociado_Aportes_Atrasados_Enviar(int CodEmpresa,CoNotificaAsociadoAportesAtrasadosEnviarRequest? req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            SqlTransaction? tx = null;

            try
            {
                if (req == null)
                {
                    return DbHelper.ErrorResponse("No se recibió la solicitud.", -2);
                }

                var usuarioSesion = (req.usuario_sesion ?? string.Empty).Trim();

                var cedulas = (req.cedulas ?? new List<string>())
                    .Select(x => (x ?? string.Empty).Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (cedulas.Count == 0)
                {
                    return DbHelper.ErrorResponse("Indique algún caso a procesar!", -2);
                }

                conn.Open();
                tx = conn.BeginTransaction();

                foreach (var cedula in cedulas)
                {
                    conn.Execute(
                        "spCBR_Notifica_SociosAportesAtrasados_Envia",
                        new
                        {
                            Cedula = cedula,
                            Usuario = usuarioSesion
                        },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure);
                }

                tx.Commit();

                return DbHelper.OkResponse("Notificaciones Procesadas Satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                if (tx != null)
                {
                    try
                    {
                        tx.Rollback();
                    }
                    catch (InvalidOperationException rollbackEx)
                    {
                        return DbHelper.ErrorResponse(
                            $"{ex.Message} | Error al revertir transacción: {rollbackEx.Message}",
                            -1);
                    }
                    catch (SqlException rollbackEx)
                    {
                        return DbHelper.ErrorResponse(
                            $"{ex.Message} | Error al revertir transacción: {rollbackEx.Message}",
                            -1);
                    }
                }

                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }
    }
}