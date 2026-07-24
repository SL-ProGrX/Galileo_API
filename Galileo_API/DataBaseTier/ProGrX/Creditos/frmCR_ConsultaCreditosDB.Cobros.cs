using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Cobros

        /// <summary>
        /// Obtiene los cobros de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCobroDto>> CR_ConsultaCobros_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<CrConsultaCobroDto>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
        S.*,
        ISNULL(G.descripcion, '') AS Gestion,
        ISNULL(C.descripcion, '') AS Causa,
        ISNULL(A.descripcion, '') AS Arreglo,
        DATEADD(day, ISNULL(S.tiempo_resolucion, 0), S.fecha) AS comision_vence
                FROM CBR_Seguimiento S
                LEFT JOIN cbr_gestiones G 
                    ON S.cod_gestion = G.cod_gestion
                LEFT JOIN CBR_CAUSAS_MOROSIDAD C 
                    ON S.cod_causa = C.cod_causa
                LEFT JOIN CBR_TIPOS_ARREGLOS A 
                    ON S.cod_arreglo = A.cod_arreglo
                WHERE S.cedula = @Cedula
                ORDER BY S.cod_seg DESC;",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Consulta Asignacion de Oficina de Cobro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaAsignacionCobroData>> CR_ConsultaAsignacion_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<CrConsultaAsignacionCobroData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
                    usuario,
                    cedula,
                    fecha_asignacion,
                    mantener,
                    rebajo_doble,
                    aplica_mora
                FROM CBR_Asignacion_H
                WHERE cedula = @Cedula
                ORDER BY fecha_asignacion DESC;",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Procesa la notificación de cobros por email de una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="tipo">Tipo de notificación que se debe procesar.</param>
        /// <param name="usuario">Usuario que registra el movimiento.</param>
        /// <returns>Resultado del procesamiento de la notificación.</returns>
        public ErrorDto CR_ConsultaCobros_NotificacionEmail_Procesar(
            int codEmpresa,
            string cedula,
            string tipo,
            string usuario)
        {
            var resultado = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    "exec spSys_Notifica_Cobros_01_Atrasos @cedula, @tipo, @usuario",
                    new
                    {
                        cedula = (cedula ?? string.Empty).Trim(),
                        tipo = string.Equals(tipo, "D", StringComparison.OrdinalIgnoreCase) ? "D" : "R",
                        usuario = (usuario ?? string.Empty).Trim()
                    });

                return true;
            });

            return resultado.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(
                    resultado.Description ?? "Error al procesar la notificación por email.",
                    resultado.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}
