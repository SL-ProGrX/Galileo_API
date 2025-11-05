using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_UsuariosDB
    {
        private readonly IConfiguration _config;

        public frmPres_UsuariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los usuarios del módulo 
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="Usuario">Nombre del usuario (opcional)</param>
        /// <returns>Lista de usuarios del módulo </returns>
        public ErrorDTO<List<PresUsuariosData>> Pres_Usuarios_Obtener(int CodEmpresa, string? Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<PresUsuariosData>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Usuarios_Modulo '%{Usuario}%'";
                    resp.Result = connection.Query<PresUsuariosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }


        /// <summary>
        /// Obtiene las contabilidades 
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="Usuario">Nombre del usuario</param>
        /// <returns>Lista de contabilidades </returns>
        public ErrorDTO<List<PresContabilidadesData>> Pres_Contabilidades_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<PresContabilidadesData>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Usuarios_Modulo_Contabilidades '{Usuario}'";
                    resp.Result = connection.Query<PresContabilidadesData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene las unidades del módulo
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="Usuario">Nombre del usuario</param>
        /// <param name="CodContab">Código de la contabilidad</param>   
        /// <returns>Lista de unidades del módulo</returns>
        public ErrorDTO<List<PresUnidadesData>> Pres_Unidades_Obtener(int CodEmpresa, string Usuario, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<PresUnidadesData>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Usuarios_Modulo_Unidades '{Usuario}', {CodContab}";
                    resp.Result = connection.Query<PresUnidadesData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Registra o actualiza un usuario en el módulo
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="request">Objeto con los datos del usuario a registrar o actualizar</param> 
        /// /// <returns>Objeto ErrorDTO con el código y descripción del resultado de la operación</returns>
        public ErrorDTO Pres_Usuarios_Registro_SP(int CodEmpresa, PresUsuariosInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            int activoValue;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Activo != true)
                    {
                        activoValue = 0;
                    }
                    else
                    {
                        activoValue = 1;
                    }
                    var query = $@"exec spPres_Usuarios_Modulo_Registro '{request.Usuario}','{request.UserReg}',{activoValue}";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Usuario modificado satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Registra o actualiza una unidad en el módulo
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>       
        /// /// <param name="request">Objeto con los datos de la unidad a registrar o actualizar</param>
        /// <returns>Objeto ErrorDTO con el código y descripción del resultado de la operación</returns>
        public ErrorDTO Pres_Unidades_Registro_SP(int CodEmpresa, PresUnidadesInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            int activoValue;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (request.Activo != true)
                    {
                        activoValue = 0;
                    }
                    else
                    {
                        activoValue = 1;
                    }
                    var query = $@"exec spPres_Usuarios_Unidades_Registro '{request.Usuario}',{request.Cod_Contabilidad},
                        '{request.Cod_Unidad}','{request.UserReg}',{activoValue}";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Nivel modificado satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}