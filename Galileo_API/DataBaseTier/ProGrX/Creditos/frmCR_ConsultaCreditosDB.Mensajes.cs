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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<AfiSociosMensajesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiSociosMensajesData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                        SELECT *
                        FROM socios_mensajes
                        WHERE cedula = @Cedula
                          AND DATEDIFF(DAY, dbo.MyGetdate(), vencimiento) >= 0
                          AND Tipo = @Tipo
                          AND ISNULL(Resolucion, 'P') = 'P'
                        ORDER BY Fecha DESC;
                    ";

                response.Result = connection
                    .Query<AfiSociosMensajesData>(query, new
                    {
                        Cedula = cedula,
                        Tipo = tipo
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDto AFI_Socios_Mensajes_Guardar(int codEmpresa, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;

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
                           AND Tipo = 'G'
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
                        mensajeOriginal = data.mensaje_original
                    });

                    if (filas == 0)
                    {
                        response.Code = -1;
                        response.Description = "No fue posible localizar el mensaje original para actualizarlo.";
                    }
                }
                else
                {
                    const string query = @"
                        INSERT INTO socios_mensajes
                            (fecha, cedula, usuario, vencimiento, mensaje, Tipo)
                        VALUES
                            (dbo.MyGetdate(), @cedula, @usuario, @fechaVence, @mensaje, 'G');";

                    connection.Execute(query, new
                    {
                        cedula = data.cedula,
                        usuario = data.usuario,
                        fechaVence = vfecha,
                        mensaje = data.mensaje
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto AFI_Socios_Mensajes_Elimina(int codEmpresa, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                       delete from socios_mensajes 
                       where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = 'G'
                         and resolucion = 'P'
                    ";

                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;

                connection.ExecuteAsync(query, new
                {
                    cedula = data.cedula,
                    usuario = data.usuario,
                    fecha = vfecha,
                    mensaje = data.mensaje
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa, string usuario, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                       update socios_mensajes set Resolucion = 'R', Resolucion_Fecha = dbo.MyGetdate()
                          , Resolucion_Usuario = @usuario
                           where cedula = @cedula 
                           and usuario = @userMsj
                           and vencimiento = @fecha_vence
                           and substring(mensaje,1,15) = substring(@mensaje,1,15)";

                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;

                connection.ExecuteAsync(query, new
                {
                    cedula = data.cedula,
                    usuario = usuario,
                    userMsj = data.usuario,
                    fecha_vence = vfecha,
                    mensaje = data.mensaje
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion
    }
}
