using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR; 
using PgxAPI.Models.ProGrX_Nucleo; 
using System.Data;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_RA_AutorizacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; 
        private readonly mSecurityMainDb _Security_MainDB;


        public frmSYS_RA_AutorizacionesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
       /// <summary>
       /// Consulta de usaurios autorizados
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                 using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select USUARIO as 'item', NOMBRE as descripcion from vSYS_RA_Usuarios_Autorizados  ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
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
        /// Consulta listado de casos de autorizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<SysAutorizacionesData>> SYS_RA_AutorizacionesCasos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<SysAutorizacionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysAutorizacionesData>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"SELECT Persona_Id, Cedula, NOMBRE,Estado FROM vSYS_RA_Casos ";
                    result.Result = connection.Query<SysAutorizacionesData>(query).ToList();


                }
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
        /// Consulta de datos de un caso autorizado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="persona_id"></param>
        /// <returns></returns>
        public ErrorDTO<SysAutorizacionesData> SYS_RA_AutorizacionesCasosDatos_Obtener(int CodEmpresa,int persona_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SysAutorizacionesData>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysAutorizacionesData()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select *, isnull(Fecha_Vence, '2300/01/01') as 'Fecha_Vence_Id' from vSYS_RA_Casos where Persona_Id = @id ";
                    result.Result = connection.Query<SysAutorizacionesData>(query, new { id = persona_id }).FirstOrDefault();


                }
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
        /// Metodo para guardar autorizacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <param name="clave"></param>
        /// <returns></returns>
        public ErrorDTO SYS_RA_Autorizaciones_Autorizar(int CodEmpresa, string usuario, SysAutorizacionesData datos,string clave)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    int AUTORIZACION_ID = connection.QuerySingle<int>(
                                "spSYS_RA_Autorizacion",
                                new {
                                    PersonaId = datos.persona_id,
                                    Horas = datos.horas,
                                    Usuario = datos.usuario_autorizado,
                                    Notas = datos.notas,
                                    Aut_Usuario = usuario,
                                    Aut_Clave =clave
                                },
                            commandType: CommandType.StoredProcedure
                            );
                    result.Code = AUTORIZACION_ID;

                    if (AUTORIZACION_ID > 0)
                    {
                        _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Autorización: {AUTORIZACION_ID} Expediente Restringido: {datos.persona_id} Cedula = {datos.cedula}",
                            Movimiento = "Registra - WEB",
                            Modulo = vModulo
                        });
                    }
                  
                   
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}
