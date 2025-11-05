using PgxAPI.Models.TES;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using PgxAPI.Models;



namespace PgxAPI.DataBaseTier.TES
{
    public class frmTES_TokenDB
    {
        private readonly IConfiguration? _config;
        private mTesoreria _mtes;

        public frmTES_TokenDB(IConfiguration? config)
        {
            _config = config;
            _mtes = new mTesoreria(_config);
        }

        /// <summary>
        /// Obtiene los primeros 100 tokens de la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tokens.</returns>
        public ErrorDTO<List<TES_TokenDTO>> TES_Token_Top_Obtener(int CodEmpresa)
        {
            if (_config == null)
            {
                throw new ArgumentNullException(nameof(_config), "Configuración es nula");
            }

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_TokenDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"
                                    SELECT TOP 100 
                                        Tok.ID_TOKEN AS IdToken,
                                        Tok.ESTADO AS Estado,
                                        Tok.REGISTRO_FECHA AS RegistroFecha,
                                        Tok.REGISTRO_USUARIO AS RegistroUsuario,
                                        ISNULL(COUNT(*), 0) AS Pendiente,
                                        ISNULL(SUM(Tra.Monto), 0) AS Monto
                                    FROM TES_TOKENS Tok
                                    LEFT JOIN TES_TRANSACCIONES Tra ON Tok.ID_TOKEN = Tra.ID_TOKEN AND Tra.ESTADO = 'P'
                                    GROUP BY Tok.ID_TOKEN, Tok.ESTADO, Tok.REGISTRO_FECHA, Tok.REGISTRO_USUARIO
                                    ORDER BY Tok.REGISTRO_FECHA DESC";

                    response.Result = connection.Query<TES_TokenDTO>(query).ToList();
                }
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
        /// Cierra un token en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Id">ID del token a cerrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDTO TES_Token_Cerrar(int CodEmpresa, string Id)
        {
            if (_config == null)
            {
                throw new ArgumentNullException(nameof(_config), "Configuración es nula");
            }

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Verificar si el token está activo
                    string querySelect = "SELECT id_token FROM tes_tokens WHERE estado = 'A' AND id_token = @Id";
                    var tokenActivo = connection.QueryFirstOrDefault<string>(querySelect, new { Id });

                    if (tokenActivo == null)
                    {
                        response.Code = -2;
                        response.Description = "Este token ya está cerrado.";
                        return response;
                    }

                    // Cerrar el token
                    string queryUpdate = "UPDATE tes_tokens SET estado = 'C' WHERE id_token = @Id";
                    connection.Execute(queryUpdate, new { Id });

                    response.Description = "Token cerrado satisfactoriamente.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }
    
        public ErrorDTO<List<TES_TokenSolicitudesData>> TES_Token_Pen_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_TokenSolicitudesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TES_TokenSolicitudesData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Verificar si el token está activo
                    var procedure = $@"[spPres_TokenPendientes_Obtener]";

                    response.Result = connection.Query<TES_TokenSolicitudesData>(procedure, commandType: System.Data.CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDTO TES_Token_Pen_Incluir(int CodEmpresa, string token ,List<string> solicitudes)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var solicitud in solicitudes)
                    {
                       
                        var procedure = $@"[spPres_TokenPendientes_Actualizar]";
                        var values = new
                        {
                            token = token,
                            nSolicitid = solicitud
                        };
                        try
                        {
                            var error = connection.Query<object>(procedure, values, commandType: System.Data.CommandType.StoredProcedure);
                        }
                        catch (Exception e)
                        {
                            response.Code = -1;
                            response.Description += solicitud + ": " + e.Message + "\n";
                        }
                        
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDTO TES_Token_Crear(int CodEmpresa, string Usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, Usuario);
        }
    }
}