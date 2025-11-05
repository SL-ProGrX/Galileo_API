
using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR; 
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient; 

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_RA_UsuariosDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSYS_RA_UsuariosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Metodo para consultar de Usuarios Autorizados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SysUsuariosData>> Sys_RA_Usuarios_Consulta(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysUsuariosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysUsuariosData>()
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSYS_RA_Usuarios_Consulta '{filtro.Trim()}'";
                    result.Result = connection.Query<SysUsuariosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<SysUsuariosData>();
            }
            return result;
        }

        /// <summary>
        /// Metodo para Activa o Inactiva a un Usuario como Autorizado a Consulta de Exp. RA
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ra_usuario"></param>
        /// <param name="usuario"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sys_RA_Usuarios_Asigna(int CodEmpresa, string ra_usuario, string usuario, bool accion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            int check = accion == true ? 1 : 0;
            string _accion = accion == true ? "Activa" : "Inactiva";
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                     var query = $@"exec spSYS_RA_Usuarios_Add @ra_usuario,@check,@usuario";
                    connection.Execute(query,
                         new
                         {
                             ra_usuario = ra_usuario.Trim(),
                             usuario = usuario, 
                             check = check
                         });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"RA Usuario Autorizado: {ra_usuario} - {_accion}",
                        Movimiento = "Aplica - WEB",
                        Modulo = vModulo
                    });
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
