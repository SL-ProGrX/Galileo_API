using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresRolesAcessoDb
    {
        private readonly IConfiguration _config;
        private const string _registroActualizado = "Registro actualizado satisfactoriamente";

        public FrmPresRolesAcessoDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para obtener los roles
        /// </summary>
        public ErrorDto<RolesLista> ObtenerRoles(int CodEmpresa, int contabilidad, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<RolesLista>
            {
                Result = new RolesLista { total = 0 }
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Rol_List";

                var lista = connection.Query<RolesDto>(
                    proc,
                    new { contabilidad, usuario },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                resp.Result.lista = lista;
                resp.Result.total = lista.Count;
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
        /// Método para insertar o actualizar un rol
        /// </summary>
        public ErrorDto Roles_Upsert(int CodCliente, string usuario, RolesDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                int activo = request.activo ? 1 : 0;
                int control = request.control ? 1 : 0;

                using var connection = new SqlConnection(clienteConnString);

                const string sqlExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe 
                    FROM PRES_AC_ROLES 
                    WHERE COD_ROL = @CodRol";

                int existe = connection.Query<int>(
                    sqlExiste,
                    new { CodRol = request.cod_rol }
                ).FirstOrDefault();

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        INSERT INTO PRES_AC_ROLES
                            (COD_ROL, COD_CONTABILIDAD, DESCRIPCION, CONTROL, ACTIVO, Registro_Fecha, Registro_Usuario) 
                        VALUES
                            (@CodRol, @CodContabilidad, @Descripcion, @Control, @Activo, GETDATE(), @Usuario)";

                    var parametrosInsert = new
                    {
                        CodRol = request.cod_rol,
                        CodContabilidad = request.cod_contabilidad,
                        Descripcion = request.descripcion,
                        Control = control,
                        Activo = activo,
                        Usuario = usuario
                    };

                    connection.Execute(sqlInsert, parametrosInsert);
                    resp.Description = "Registro agregado satisfactoriamente";
                }
                else
                {
                    const string sqlUpdate = @"
                        UPDATE PRES_AC_ROLES 
                        SET DESCRIPCION = @Descripcion,
                            CONTROL      = @Control,
                            ACTIVO       = @Activo
                        WHERE COD_CONTABILIDAD = @CodContabilidad 
                          AND COD_ROL         = @CodRol";

                    var parametrosUpdate = new
                    {
                        CodRol = request.cod_rol,
                        CodContabilidad = request.cod_contabilidad,
                        Descripcion = request.descripcion,
                        Control = control,
                        Activo = activo
                    };

                    connection.Execute(sqlUpdate, parametrosUpdate);
                    resp.Description = _registroActualizado;
                }

                resp.Code = 0;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para obtener los miembros de un rol
        /// </summary>
        public ErrorDto<List<MiembrosRolDto>> Rol_Miembros_Obtener(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string? filtro,
            string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<MiembrosRolDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Miembros_Consulta";

                resp.Result = connection.Query<MiembrosRolDto>(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        filtro,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
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
        /// Método para asignar o eliminar un miembro de un rol
        /// </summary>
        public ErrorDto Core_Miembros_Registro(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string usuario,
            MiembrosRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            var mov = (request.asignado ?? false) ? "A" : "E";

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Miembros_Asigna";

                connection.Execute(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        UsuarioMiembro = request.usuario,
                        UsuarioRegistra = usuario,
                        Movimiento = mov
                    },
                    commandType: CommandType.StoredProcedure
                );

                resp.Description = _registroActualizado;
            }
            catch (Exception ex)
            {
                resp.Code = 0; // tu código original ponía 0 incluso en error
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para obtener las cuentas de un rol
        /// </summary>
        public ErrorDto<List<CuentaRolDto>> Rol_Cuentas_Obtener(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string? filtro,
            string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CuentaRolDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Cuentas_Consulta";

                resp.Result = connection.Query<CuentaRolDto>(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        filtro1 = string.Empty,
                        filtro2 = string.Empty,
                        filtro3 = string.Empty,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
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
        /// Método para obtener las cuentas ya asignadas a un rol
        /// </summary>
        public ErrorDto<List<CuentaRolDto>> Rol_CuentasRegistrada_Obtener(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string? filtro,
            string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CuentaRolDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Cuentas_Consulta_Asignadas";

                resp.Result = connection.Query<CuentaRolDto>(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        filtro,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
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
        /// Método para asignar una cuenta a un rol
        /// </summary>
        public ErrorDto Rol_Cuenta_Registra(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            CuentaRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            const string mov = "A";

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Cuentas_Asigna";

                connection.Execute(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        CodCuentaMask = request.cod_cuenta_mask,
                        UsuarioRegistra = request.user_registra,
                        Movimiento = mov
                    },
                    commandType: CommandType.StoredProcedure
                );

                resp.Description = _registroActualizado;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para eliminar una cuenta de un rol
        /// </summary>
        public ErrorDto Rol_Cuenta_Elimina(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            CuentaRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            const string sql = @"
                DELETE FROM PRES_AC_CUENTAS 
                WHERE COD_CONTABILIDAD = @CodContabilidad 
                  AND COD_CUENTA       = @CodCuenta";

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                connection.Execute(
                    sql,
                    new
                    {
                        CodContabilidad = cod_contabilidad,
                        CodCuenta = request.cod_cuenta
                    });

                resp.Description = _registroActualizado;
            }
            catch (Exception ex)
            {
                resp.Code = 0; // igual que tu código original
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para eliminar un rol
        /// </summary>
        public ErrorDto Rol_Eliminar(int CodEmpresa, string codRol)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0 };

            const string sql = @"
                DELETE FROM PRES_AC_ROLES 
                WHERE COD_ROL = @CodRol";

            try
            {
                using var connection = new SqlConnection(stringConn);

                connection.Execute(sql, new { CodRol = codRol });

                resp.Description = "Rol eliminado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para obtener las unidades de un rol
        /// </summary>
        public ErrorDto<List<UnidadesRolDto>> Rol_Unidades_Obtener(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string? filtro,
            string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<UnidadesRolDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Unidades_Consulta";

                resp.Result = connection.Query<UnidadesRolDto>(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        filtro,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
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
        /// Método para asignar o eliminar una unidad a un rol
        /// </summary>
        public ErrorDto Rol_Unidad_Registro(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            int boolasingado,
            UnidadesRolDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            var movimiento = boolasingado == 1 ? "A" : "E";

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Unidades_Asigna";

                connection.Execute(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        CodUnidad = request.cod_unidad,
                        UsuarioRegistra = request.user_registra,
                        Movimiento = movimiento
                    },
                    commandType: CommandType.StoredProcedure
                );

                resp.Description = _registroActualizado;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Método para obtener los centros de costo de un rol
        /// </summary>
        public ErrorDto<List<CentroCosto>> Rol_Unidad_CC_Obtener(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string unidad,
            string? filtro,
            string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto<List<CentroCosto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string proc = "spPres_AC_Centro_Costo_Consulta";

                resp.Result = connection.Query<CentroCosto>(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        unidad,
                        filtro,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
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
        /// Método para asignar o eliminar un centro de costo a una unidad de un rol
        /// </summary>
        public ErrorDto Rol_Unidad_CC_Registro(
            int CodCliente,
            string cod_contabilidad,
            string rol,
            string unidad,
            int boolasingado,
            CentroCosto request)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            var movimiento = boolasingado == 1 ? "A" : "E";

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string proc = "spPres_AC_Centro_Costo_Asigna";

                connection.Execute(
                    proc,
                    new
                    {
                        cod_contabilidad,
                        rol,
                        unidad,
                        CodCentroCosto = request.cod_centro_costo,
                        UsuarioRegistra = request.user_registra,
                        Movimiento = movimiento
                    },
                    commandType: CommandType.StoredProcedure
                );

                resp.Description = _registroActualizado;
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