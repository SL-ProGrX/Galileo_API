using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.ProGrX_Personas;
using System.Data;
using System.Reflection;
using System.Text;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Afiliacion_TagsDB
    {
        private readonly IConfiguration? _config;

        public frmAF_Afiliacion_TagsDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene Recepcion Afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recepcion(int CodEmpresa, string estado, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AfiAfiliacionControlDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiAfiliacionControlDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var result = connection.Query<AfiAfiliacionControlDto>(
                        "spAFI_Afiliaciones_Control_Consulta",
                        new { Estado = estado, Filtro = filtro },
                        commandType: CommandType.StoredProcedure).ToList();

                    response.Result = result;
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
        /// Obtiene afiliaciones recibidas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recibidas(int CodEmpresa, string estado, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AfiAfiliacionControlDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiAfiliacionControlDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var result = connection.Query<AfiAfiliacionControlDto>(
                        "spAFI_Afiliaciones_Control_Consulta",
                        new { Estado = estado, Filtro = filtro },
                        commandType: CommandType.StoredProcedure).ToList();

                    response.Result = result;
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
        /// Obtiene Afiliaciones pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Pendientes(int CodEmpresa, string estado, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AfiAfiliacionControlDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiAfiliacionControlDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var result = connection.Query<AfiAfiliacionControlDto>(
                        "spAFI_Afiliaciones_Control_Consulta",
                        new { Estado = estado, Filtro = filtro },
                        commandType: CommandType.StoredProcedure).ToList();

                    response.Result = result;
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
       /// Obtiene boletas afiliaciones
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <returns></returns>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_CR_BoletasAfiliacion_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AfBoletasAfiliacion>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfBoletasAfiliacion>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sb = new StringBuilder(@"select CONSEC, CEDULA, NOMBRE, Tipo_Desc 
                                                From vAFI_Afiliaciones_Pendientes_Recibir");

                result.Result = connection.Query<AfBoletasAfiliacion>(sb.ToString()).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Aplica recepcion 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="boleta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AFI_Afiliacion_Recepcion_Aplica(int codEmpresa, int boleta, string usuario)
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

                var sql = @"EXEC spAFI_Afiliacion_Recepcion_Aplica @CodBoleta, @Usuario, @Nota, @Maquina, @AppVersion";

                connection.Execute(sql, new
                {
                    CodBoleta = boleta,
                    Usuario = usuario,
                    Nota = $"Recibe Afiliacion No. {boleta}",
                    Maquina = "",
                    AppVersion = "Galileo"
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
        /// Aplica revision
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="consec"></param>
        /// <param name="estado"></param>
        /// <param name="usuario"></param>
        /// <param name="nota"></param>
        /// <returns></returns>
        public ErrorDto AFI_Afiliacion_Revision_Aplica(int codEmpresa, int consec, string estado, string usuario, string nota)
        {
            var response = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sql = @"EXEC spAFI_Afiliacion_Revision_Aplica 
                        @Consec, 
                        @Estado, 
                        @Usuario, 
                        @Nota";

                connection.Execute(sql, new
                {
                    Consec = consec,
                    Estado = estado,
                    Usuario = usuario,
                    Nota = nota ?? "",
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
        /// Consulta etiquetas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="boleta"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiEtiquetaDto>> AFI_Afiliaciones_Etiquetas_Consulta(int CodEmpresa, int boleta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfiEtiquetaDto>> { Code = 0, Result = new List<AfiEtiquetaDto>() };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var sql = "EXEC spAFI_Afiliaciones_Etiquetas_Consulta @BoletaId";

                var result = connection.Query<AfiEtiquetaDto>(sql, new { BoletaId = boleta }).ToList();
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Aplica revision y reversion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="boleta"></param>
        /// <param name="usuario"></param>
        /// <param name="nota"></param>
        /// <returns></returns>
        public ErrorDto AFI_Afiliacion_Revision_Reversar(int CodEmpresa, int boleta, string usuario, string nota)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Validar primero
                var valida = connection.QuerySingle<int>(
                    "select dbo.fxAFI_Afiliacion_Revision_Reversar_Valida(@boleta)",
                    new { boleta }
                );

                if (valida == 0)
                {
                    response.Code = -2;
                    response.Description = "No procede la reversión, la afiliación ya fue remesada";
                    return response;
                }

                // Ejecutar reversión
                string sql = @"exec spAFI_Afiliacion_Revision_Reversar @boleta, @usuario, @nota";
                connection.Execute(sql, new
                {
                    boleta,
                    usuario,
                    nota
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
        /// Agrega recepcion afiliacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="boleta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AFI_Afiliacion_Recepcion_Agregar(int CodEmpresa, int boleta, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string sql = @"EXEC spAFI_Afiliacion_Recepcion_Aplica @Boleta, @Usuario";

                connection.Execute(sql, new
                {
                    Boleta = boleta,
                    Usuario = usuario
                });

                response.Description = "Boleta agregada correctamente";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene lista de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_BoletasAfiliacionLista_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AfBoletasAfiliacion>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfBoletasAfiliacion>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sb = new StringBuilder(@"select CONSEC, CEDULA, NOMBRE, tipo_desc 
                                                    From vAFI_Afiliaciones_List");

                result.Result = connection.Query<AfBoletasAfiliacion>(sb.ToString()).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


    }

}
