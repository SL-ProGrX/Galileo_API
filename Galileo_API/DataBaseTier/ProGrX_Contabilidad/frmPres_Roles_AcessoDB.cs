using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_Roles_AcessoDB
    {
        private readonly IConfiguration _config;
        private readonly int vModule = 12;

        public frmPres_Roles_AcessoDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener los roles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<RolesLista> ObtenerRoles(int CodEmpresa, int contabilidad, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<RolesLista>();
            resp.Result = new RolesLista();
            resp.Result.total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(clienteConnString);
                {

                    query = "exec spPres_AC_Rol_List @contabilidad, @usuario";
                    resp.Result.lista = connection.Query<RolesDto>(query, new { contabilidad = contabilidad, usuario = usuario }).ToList();

                    resp.Result.total = resp.Result.lista.Count;
                    //Busco Total
                    //query = $@"Select COUNT(cod_rol) from PRES_AC_ROLES";
                    //resp.Result.total = connection.Query<int>(query).FirstOrDefault();

                    //if (filtro != null)
                    //{
                    //    filtro = " WHERE cod_rol LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    //}

                    //if (pagina != null)
                    //{
                    //    paginaActual = " OFFSET " + pagina + " ROWS ";
                    //    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    //}

                    //query = $@"Select cod_rol,cod_contabilidad,descripcion, control,activo,registro_fecha,registro_usuario  
                    //            from PRES_AC_ROLES {filtro} order by cod_rol 
                    //            {paginaActual} {paginacionActual}";
                    //resp.Result.lista = connection.Query<RolesDto>(query).ToList();

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
        /// Metodo para insertar o actualizar un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Roles_Upsert(int CodCliente, string usuario, RolesDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            var activo = 0;
            var control = 0;

            try
            {
                if (request.activo == true)
                {
                    activo = 1;
                }

                if (request.control == true)
                {
                    control = 1;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from PRES_AC_ROLES where COD_ROL = '{request.cod_rol}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0)
                    {

                        query = @$"insert 
                            into PRES_AC_ROLES(COD_ROL, COD_CONTABILIDAD, DESCRIPCION, CONTROL, ACTIVO, Registro_Fecha, Registro_Usuario) 
                            values('{request.cod_rol}','{request.cod_contabilidad}','{request.descripcion}', '{control}',
                               {activo}, Getdate(), '{usuario}' )";
                        resp.Description = "Registro agregado satisfactoriamente";
                    }
                    else
                    {
                        query = @$"update PRES_AC_ROLES set descripcion = '{request.descripcion}',
                            control = '{control}', activo = '{activo}'
                            where cod_contabilidad = '{request.cod_contabilidad}' and cod_rol = '{request.cod_rol}' ";
                        resp.Description = "Registro actualizado satisfactoriamente";
                    }
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;

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
        /// Metodo para obtener los miembros de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="filtro"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<MiembrosRolDto>> Rol_Miembros_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<MiembrosRolDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Miembros_Consulta] '{cod_contabilidad}','{rol}','{filtro}', '{usuario}'";
                    resp.Result = connection.Query<MiembrosRolDto>(query).ToList();
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
        /// Metodo para asignar o eliminar un miembro de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_Miembros_Registro(int CodCliente, string cod_contabilidad, string rol, string usuario, MiembrosRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            var mov = 'E';
            try
            {
                if (request.asignado == true)
                {
                    mov = 'A';
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Miembros_Asigna] '{cod_contabilidad}', '{rol}', '{request.usuario}', '{usuario}', '{mov}'";
                    resp.Description = "Registro actualizado satisfactoriamente";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Metodo para obtener las cuentas de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="filtro"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaRolDto>> Rol_Cuentas_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CuentaRolDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Cuentas_Consulta] '{cod_contabilidad}','{rol}' ,'','','', '{usuario}'";
                    resp.Result = connection.Query<CuentaRolDto>(query).ToList();
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
        /// Metodo para obtener las cuentas ya asignadas a un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="filtro"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaRolDto>> Rol_CuentasRegistrada_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CuentaRolDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Cuentas_Consulta_Asignadas] '{cod_contabilidad}','{rol}','', '{usuario}'";
                    resp.Result = connection.Query<CuentaRolDto>(query).ToList();
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
        /// Metodo para asignar una cuenta a un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Rol_Cuenta_Registra(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            var mov = 'A';
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Cuentas_Asigna] '{cod_contabilidad}', '{rol}', '{request.cod_cuenta_mask}', '{request.user_registra}', '{mov}'";
                    resp.Description = "Registro actualizado satisfactoriamente";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;
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
        /// Metodo para eliminar una cuenta de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Rol_Cuenta_Elimina(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            rol = "0" + rol;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"delete from PRES_AC_CUENTAS where COD_CONTABILIDAD = '{cod_contabilidad}' AND COD_CUENTA = '{request.cod_cuenta}'";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;
                }

            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Metodo para eliminar un rol
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codRol"></param>
        /// <returns></returns>
        public ErrorDto Rol_Eliminar(int CodEmpresa, string codRol)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete PRES_AC_ROLES where COD_ROL = '{codRol}' ";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;
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
        /// Metodo para obtener las unidades de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="filtro"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<UnidadesRolDto>> Rol_Unidades_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {


            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<UnidadesRolDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Unidades_Consulta] '{cod_contabilidad}','{rol}' ,'', '{usuario}'";
                    resp.Result = connection.Query<UnidadesRolDto>(query).ToList();
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
        /// Metodo para asignar o eliminar una unidad a un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="boolasingado"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Rol_Unidad_Registro(int CodCliente, string cod_contabilidad, string rol, int boolasingado, UnidadesRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            var movimiento = boolasingado == 1 ? "A" : "E";

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Unidades_Asigna] '{cod_contabilidad}', '{rol}', '{request.cod_unidad}', '{request.user_registra}', '{movimiento}'";
                    resp.Description = "Registro actualizado satisfactoriamente";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Code = 0;
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
        /// Metodo para obtener los centros de costo de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="unidad"></param>
        /// <param name="filtro"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CentroCosto>> Rol_Unidad_CC_Obtener(int CodCliente, string cod_contabilidad, string rol, string unidad, string? filtro, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CentroCosto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec [spPres_AC_Centro_Costo_Consulta] '{cod_contabilidad}','{rol}' ,'{unidad}','', '{usuario}'";
                    resp.Result = connection.Query<CentroCosto>(query).ToList();
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
        /// Metodo para asignar o eliminar un centro de costo a una unidad de un rol
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="rol"></param>
        /// <param name="unidad"></param>
        /// <param name="boolasingado"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto Rol_Unidad_CC_Registro(int CodCliente, string cod_contabilidad, string rol, string unidad, int boolasingado, CentroCosto request)
        {
            if (_config == null)
            {
                throw new ArgumentNullException(nameof(_config), "Configuración es nula");
            }

            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto
            {
                Code = 0
            };

            var movimiento = boolasingado == 1 ? "A" : "E";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"exec [spPres_AC_Centro_Costo_Asigna] '{cod_contabilidad}', '{rol}', '{unidad}', '{request.cod_centro_costo}', '{request.user_registra}', '{movimiento}'";         
                    connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente";
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