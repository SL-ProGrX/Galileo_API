using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_TelefonosDB
    {
        private readonly IConfiguration _config;

        public frmAF_TelefonosDB(IConfiguration config)
        {
            _config = config;
        }
        
        /// <summary>
        /// Obtener tipos de telefonos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_TiposTelefonos_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
                    SELECT idTipoTelefono AS item,
                           nombreTipoTelefono AS descripcion
                    FROM AFI_TIPOS_TELEFONOS
                    WHERE Activo = 1
                    ORDER BY Prioridad;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener telefonos por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_TelefonoDTO>> AF_Telefonos_ObtenerPorCedula(int CodEmpresa, string cedula)
        {
            var response = new ErrorDTO<List<AF_TelefonoDTO>> { Code = 0, Result = new() };
            try
            {
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
            SELECT 
                T.Telefono,
                T.Cedula,
                T.Contacto,
                T.Numero,
                T.Tipo,
                T.Ext,
                T.Usuario,
                T.Fecha
            FROM Telefonos T
            WHERE cedula = @Cedula;";

                response.Result = connection.Query<AF_TelefonoDTO>(query, new { Cedula = cedula }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Insertar telefono
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipoId"></param>
        /// <param name="numero"></param>
        /// <param name="ext"></param>
        /// <param name="contacto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_Telefono_Insertar(int CodEmpresa, string cedula, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {
                if (ext == "0")
                {
                    ext = null;
                }
                if (contacto == "N/A")
                {
                    contacto = null;
                }
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string insertQuery = @"
                    INSERT INTO telefonos (cedula, tipo, numero, ext, contacto, usuario, fecha)
                    VALUES (@Cedula, @TipoId, @Numero, @Ext, @Contacto, @Usuario, dbo.MyGetDate());";

                connection.Execute(insertQuery, new
                {
                    Cedula = cedula,
                    TipoId = tipoId,
                    Numero = numero,
                    Ext = ext,
                    Contacto = contacto,
                    Usuario = usuario
                });

                const string selectQuery = @"SELECT MAX(telefono) FROM telefonos WHERE cedula = @Cedula;";
                int ultimoId = connection.QueryFirstOrDefault<int>(selectQuery, new { Cedula = cedula });


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;

            }
            return response;
        }

        /// <summary>
        /// Actualizar telefono
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="telefonoId"></param>
        /// <param name="tipoId"></param>
        /// <param name="numero"></param>
        /// <param name="ext"></param>
        /// <param name="contacto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_Telefono_Actualizar(int CodEmpresa, int telefonoId, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            var response = new ErrorDTO { Code = 0, Description = "Actualizado correctamente" };
            try
            {
                if (ext == "0")
                {
                    ext = null;
                }
                if (contacto == "N/A")
                {
                    contacto = null;
                }
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string updateQuery = @"
                    UPDATE telefonos
                    SET numero = @Numero,
                        ext = @Ext,
                        contacto = @Contacto,
                        tipo = @TipoId,
                        usuario = @Usuario,
                        fecha = dbo.MyGetDate()
                    WHERE telefono = @TelefonoId;";

                connection.Execute(updateQuery, new
                {
                    Numero = numero,
                    Ext = ext,
                    Contacto = contacto,
                    TipoId = tipoId,
                    Usuario = usuario,
                    TelefonoId = telefonoId
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Eliminar teléfono
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="telefonoId"></param>
        /// <returns></returns>
        public ErrorDTO AF_Telefono_Eliminar(int CodEmpresa, int telefonoId)
        {
            var response = new ErrorDTO { Code = 0, Description = "Eliminado correctamente" };
            try
            {
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string deleteQuery = @"DELETE FROM telefonos WHERE telefono = @TelefonoId;";
                connection.Execute(deleteQuery, new { TelefonoId = telefonoId });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
