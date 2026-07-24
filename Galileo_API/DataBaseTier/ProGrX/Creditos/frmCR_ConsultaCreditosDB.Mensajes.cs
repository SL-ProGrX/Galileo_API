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
        #region Mensajes
        /// <summary>
        /// Obtiene los mensajes de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiSociosMensajesData>> AFI_Socios_Mensajes_Obtener(int codEmpresa, string cedula, string tipo)
        {
            const string query = @"
                        SELECT *
                        FROM socios_mensajes
                        WHERE cedula = @Cedula
                          AND DATEDIFF(DAY, dbo.MyGetdate(), vencimiento) >= 0
                          AND Tipo = @Tipo
                          AND ISNULL(Resolucion, 'P') = 'P'
                        ORDER BY Fecha DESC;
                    ";

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<AfiSociosMensajesData>(query, new
                    {
                        Cedula = cedula,
                        Tipo = tipo
                    })
                    .ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfiSociosMensajesData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener los mensajes de la persona.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfiSociosMensajesData>());
        }

        /// <summary>
        /// Guarda o actualiza un mensaje de la persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="data">Datos del mensaje que se debe guardar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Socios_Mensajes_Guardar(int codEmpresa, AfiSociosMensajesData data)
        {
            string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;
            string tipo = string.IsNullOrWhiteSpace(data.tipo) ? "G" : data.tipo.Trim().ToUpperInvariant();

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                if (data.vencimiento_original.HasValue && !string.IsNullOrWhiteSpace(data.mensaje_original))
                {
                    const string query = @"
                        UPDATE socios_mensajes
                           SET vencimiento = @fechaVence,
                               mensaje = @mensaje
                         WHERE cedula = @cedula
                           AND vencimiento = @fechaOriginal
                           AND usuario = @usuarioOriginal
                           AND SUBSTRING(mensaje, 1, 15) = SUBSTRING(@mensajeOriginal, 1, 15)
                           AND Tipo = @tipo
                           AND ISNULL(resolucion, 'P') = 'P';";

                    string fechaOriginal = MProGrXAuxiliarDB.validaFechaGlobal(
                        data.vencimiento_original,
                        FormatoFechaIso
                    ) ?? string.Empty;

                    int filas = connection.Execute(query, new
                    {
                        cedula = data.cedula,
                        fechaVence = vfecha,
                        mensaje = data.mensaje,
                        fechaOriginal,
                        usuarioOriginal = data.usuario_original,
                        mensajeOriginal = data.mensaje_original,
                        tipo
                    });

                    return filas;
                }

                const string insertQuery = @"
                        INSERT INTO socios_mensajes
                            (fecha, cedula, usuario, vencimiento, mensaje, Tipo)
                        VALUES
                            (dbo.MyGetdate(), @cedula, @usuario, @fechaVence, @mensaje, @tipo);";

                return connection.Execute(insertQuery, new
                {
                    cedula = data.cedula,
                    usuario = data.usuario,
                    fechaVence = vfecha,
                    mensaje = data.mensaje,
                    tipo
                });
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al guardar el mensaje.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    "No fue posible localizar el mensaje original para actualizarlo.",
                    -1);
        }

        /// <summary>
        /// Elimina un mensaje pendiente de la persona.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="data">Datos que identifican el mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Socios_Mensajes_Elimina(int codEmpresa, AfiSociosMensajesData data)
        {
            const string query = @"
                       delete from socios_mensajes 
                       where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = @tipo
                         and resolucion = 'P'
                    ";

            string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;
            string tipo = string.IsNullOrWhiteSpace(data.tipo) ? "G" : data.tipo.Trim().ToUpperInvariant();
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Execute(query, new
                {
                    cedula = data.cedula,
                    usuario = data.usuario,
                    fecha = vfecha,
                    mensaje = data.mensaje,
                    tipo
                }));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al eliminar el mensaje.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra la resolución de un mensaje pendiente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que registra la resolución.</param>
        /// <param name="data">Datos que identifican el mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa, string usuario, AfiSociosMensajesData data)
        {
            const string query = @"
                       update socios_mensajes set Resolucion = 'R', Resolucion_Fecha = dbo.MyGetdate()
                          , Resolucion_Usuario = @usuario
                           where cedula = @cedula 
                           and usuario = @userMsj
                           and vencimiento = @fecha_vence
                           and substring(mensaje,1,15) = substring(@mensaje,1,15)";

            string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Execute(query, new
                {
                    cedula = data.cedula,
                    usuario = usuario,
                    userMsj = data.usuario,
                    fecha_vence = vfecha,
                    mensaje = data.mensaje
                }));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al resolver el mensaje.",
                    result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}
