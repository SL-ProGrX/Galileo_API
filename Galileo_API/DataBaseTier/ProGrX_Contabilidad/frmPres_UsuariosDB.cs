using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.DataBaseTier
{
    public class FrmPresUsuariosDb
    {
        private readonly IConfiguration _config;

        public FrmPresUsuariosDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los usuarios del módulo 
        /// </summary>
        public ErrorDto<List<PresUsuariosData>> Pres_Usuarios_Obtener(int CodEmpresa, string? Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresUsuariosData>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                // El SP se llama con un parámetro en lugar de concatenar el valor
                const string query = "exec spPres_Usuarios_Modulo @Filtro";

                var filtro = Usuario == null ? "%%" : $"%{Usuario}%";

                resp.Result = connection.Query<PresUsuariosData>(
                    query,
                    new { Filtro = filtro }
                ).ToList();

                resp.Code = 0;
                resp.Description = "OK";
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
        public ErrorDto<List<PresContabilidadesData>> Pres_Contabilidades_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresContabilidadesData>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = "exec spPres_Usuarios_Modulo_Contabilidades @Usuario";

                resp.Result = connection.Query<PresContabilidadesData>(
                    query,
                    new { Usuario }
                ).ToList();

                resp.Code = 0;
                resp.Description = "OK";
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
        public ErrorDto<List<PresUnidadesData>> Pres_Unidades_Obtener(int CodEmpresa, string Usuario, int CodContab)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresUnidadesData>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = "exec spPres_Usuarios_Modulo_Unidades @Usuario, @CodContab";

                resp.Result = connection.Query<PresUnidadesData>(
                    query,
                    new { Usuario, CodContab }
                ).ToList();

                resp.Code = 0;
                resp.Description = "OK";
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
        public ErrorDto Pres_Usuarios_Registro_SP(int CodEmpresa, PresUsuariosInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                int activoValue = (request.Activo ?? false) ? 1 : 0;

                const string query = "exec spPres_Usuarios_Modulo_Registro @Usuario, @UserReg, @Activo";

                resp.Code = connection.ExecuteAsync(
                    query,
                    new
                    {
                        Usuario = request.Usuario,
                        UserReg = request.UserReg,
                        Activo = activoValue
                    }
                ).Result;

                resp.Description = "Usuario modificado satisfactoriamente!";
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
        public ErrorDto Pres_Unidades_Registro_SP(int CodEmpresa, PresUnidadesInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                int activoValue = (request.Activo ?? false) ? 1 : 0;

                const string query = @"
                    exec spPres_Usuarios_Unidades_Registro 
                        @Usuario, 
                        @CodContabilidad,
                        @CodUnidad,
                        @UserReg,
                        @Activo";

                resp.Code = connection.ExecuteAsync(
                    query,
                    new
                    {
                        Usuario = request.Usuario,
                        CodContabilidad = request.Cod_Contabilidad,
                        CodUnidad = request.Cod_Unidad,
                        UserReg = request.UserReg,
                        Activo = activoValue
                    }
                ).Result;

                resp.Description = "Nivel modificado satisfactoriamente!";
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