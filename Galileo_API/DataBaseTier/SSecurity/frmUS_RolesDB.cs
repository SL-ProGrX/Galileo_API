using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmUsRolesDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int modulo = 13;
        private readonly MProGrXSecurityMainDb DBBitacora;

        public FrmUsRolesDb(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MProGrXSecurityMainDb(config);
        }

        public ErrorDto RolesVincular(RolesVincularDto req)
        {
            ErrorDto resp = new ErrorDto();
            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    if (req.Index == 1)
                    {
                        var empresaActual = connection.QueryFirstOrDefault<long?>(
                            "SELECT cod_empresa FROM US_Roles WHERE cod_rol = @CodRol",
                            new { req.CodRol }) ?? 0;
                        var usuarioActual = connection.QueryFirstOrDefault<string>(
                            "SELECT registro_usuario FROM US_Roles WHERE cod_rol = @CodRol",
                            new { req.CodRol }) ?? string.Empty;

                        if (req.EmpresaBitacora <= 0) req.EmpresaBitacora = empresaActual;
                        if (string.IsNullOrWhiteSpace(req.Usuario)) req.Usuario = usuarioActual;
                    }

                    switch (req.Index)
                    {
                        case 0: // Vincular
                            strSQL = "update us_roles set cod_empresa = @CodEmpresa where cod_rol = @CodRol";
                            connection.Execute(strSQL, new { req.CodEmpresa, req.CodRol });
                            _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
                            {
                                CodEmpresa = (int)(req.EmpresaBitacora > 0 ? req.EmpresaBitacora : req.CodEmpresa.GetValueOrDefault()),
                                usuario = req.Usuario,
                                vModulo = modulo,
                                strTipoMovimiento = "REGISTRA - WEB",
                                strDetalleMovimiento = $"Cliente {req.CodEmpresa} vinculado al rol: {req.CodRol}"
                            });
                            resp.Code = 0;
                            resp.Description = "Cliente Vinculado al Rol, satisfactoriamente!";
                            break;

                        case 1: // Desvincular
                            strSQL = "update us_roles set cod_empresa = null where cod_rol = @CodRol";
                            connection.Execute(strSQL, new { req.CodRol });
                            RegistrarBitacora(req, "ELIMINA", $"Cliente desvinculado del rol: {req.CodRol}");
                            resp.Code = 1;
                            resp.Description = "Cliente Desvinculado al Rol, satisfactoriamente!";
                            break;

                        default:
                            resp.Code = -1;
                            resp.Description = "Tipo de vinculación inválido.";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public List<RolesObtenerDto> RolFiltroObtener(string filtro)
        {
            List<RolesObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = "SELECT R.cod_Rol, R.descripcion, R.activo, " +
                        "CONVERT(varchar(10), ISNULL(R.cod_Empresa, 0)) + '- ' + RTRIM(ISNULL(C.Nombre_Largo, 'General')) AS 'Cliente', " +
                        "R.registro_Fecha, R.registro_Usuario " +
                        "FROM US_Roles R " +
                        "LEFT JOIN PGX_Clientes C ON R.cod_empresa = C.cod_Empresa " +
                        "WHERE R.descripcion LIKE @Filtro " +
                        "ORDER BY R.descripcion";

                    return connection.Query<RolesObtenerDto>(strSQL, new { Filtro = "%" + filtro + "%" }).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<RolesObtenerDto>();
            }
            return resp;
        }

        public ErrorDto RolGuardar(RolInsertarDto rol)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                int activo = (rol.Activo ?? false) ? 1 : 0;
                int cliente = Convert.ToInt16(rol.Cliente.Substring(0, 1));

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    var existe = connection.QueryFirstOrDefault<int>(
                        @"SELECT ISNULL(COUNT(*), 0) FROM US_Roles WHERE Cod_Rol = @CodRol",
                        new { CodRol = rol.Cod_Rol }
                    );

                    if (existe == 0) // Insertar
                    {
                        var strSQL = @"INSERT INTO US_Roles (Cod_Rol, Descripcion, Activo, Registro_Fecha, Registro_Usuario) 
                                        VALUES (@CodRol, @Descripcion, 1, GETDATE(), @RegistroUsuario)";

                        connection.Execute(strSQL, new { CodRol = rol.Cod_Rol.ToUpper(), Descripcion = rol.Descripcion, RegistroUsuario = rol.Registro_Usuario });

                        resp.Description = "Insercion Exitosa!";
                    }
                    else // Actualizar
                    {

                        var strSQL = @"UPDATE US_Roles SET Descripcion = @Descripcion, 
                                       Activo = @Activo WHERE Cod_Rol = @CodRol";

                        connection.Execute(strSQL, new { Descripcion = rol.Descripcion, Activo = activo, CodRol = rol.Cod_Rol });

                        resp.Description = "Actualizacion Exitosa!";
                    }

                    RolesVincularDto vinculo = new RolesVincularDto();
                    vinculo.CodEmpresa = cliente;
                    vinculo.CodRol = rol.Cod_Rol;
                    vinculo.Index = cliente == 0 ? 1 : 0;

                    RolesVincular(vinculo);
                }
            }
            catch (Exception ex)
            {
                resp.Code = 1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto RolEliminar(string CodRol, int codEmpresa, string usuario)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = "DELETE FROM US_Roles WHERE Cod_Rol = @CodRol";
                    var afectados = connection.Execute(strSQL, new { CodRol });
                    if (afectados > 0 && codEmpresa > 0 && !string.IsNullOrWhiteSpace(usuario))
                    {
                        _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
                        {
                            CodEmpresa = codEmpresa,
                            usuario = usuario,
                            vModulo = modulo,
                            strTipoMovimiento = "ELIMINA - WEB",
                            strDetalleMovimiento = $"Rol de Usuario: {CodRol}"
                        });
                    }

                    resp.Description = "Rol eliminado exitosamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private void RegistrarBitacora(RolesVincularDto req, string movimiento, string detalle)
        {
            var empresa = (int)(req.EmpresaBitacora > 0 ? req.EmpresaBitacora : req.CodEmpresa.GetValueOrDefault());
            if (empresa <= 0 || string.IsNullOrWhiteSpace(req.Usuario)) return;

            _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
            {
                CodEmpresa = empresa,
                usuario = req.Usuario,
                vModulo = modulo,
                strTipoMovimiento = $"{movimiento} - WEB",
                strDetalleMovimiento = detalle
            });
        }

        public List<RolesObtenerDto> RolesObtener()
        {
            List<RolesObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = "SELECT R.cod_Rol, R.descripcion, R.activo, " +
                        "CONVERT(varchar(10), ISNULL(R.cod_Empresa, 0)) + '- ' + RTRIM(ISNULL(C.Nombre_Largo, 'General')) AS 'Cliente', " +
                        "R.registro_Fecha, R.registro_Usuario " +
                        "FROM US_Roles R " +
                        "LEFT JOIN PGX_Clientes C ON R.cod_empresa = C.cod_Empresa " +
                        "ORDER BY R.descripcion";

                    return connection.Query<RolesObtenerDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<RolesObtenerDto>();
            }
            return resp;
        }

        public List<ClientesObtenerDto> ClientesObtener()
        {
            List<ClientesObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = "SELECT [COD_EMPRESA]"
                              + ",[NOMBRE_LARGO]"
                              + ",[NOMBRE_CORTO]"
                          + " FROM[PGX_Portal].[dbo].[PGX_CLIENTES]";

                    resp = connection.Query<ClientesObtenerDto>(strSQL).ToList();
                }

                resp.Insert(0, new ClientesObtenerDto { Cod_Empresa = "0", Nombre_Largo = "General", Nombre_Corto = "General" });
            }
            catch (Exception)
            {
                resp = new List<ClientesObtenerDto>();
            }
            return resp;
        }

    }
}
