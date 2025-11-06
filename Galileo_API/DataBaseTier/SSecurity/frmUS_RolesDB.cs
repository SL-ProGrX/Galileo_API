using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_RolesDB
    {
        private readonly IConfiguration _config;

        public frmUS_RolesDB(IConfiguration config)
        {
            _config = config;
        }

        public void RolesVincular(RolesVincularDto req)
        {
            ErrorDto resp = new ErrorDto();
            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    connection.Open();

                    switch (req.Index)
                    {
                        case 0: // Vincular
                            strSQL = "update us_roles set cod_empresa = @CodEmpresa where cod_rol = @CodRol";
                            connection.Execute(strSQL, new { req.CodEmpresa, req.CodRol });
                            resp.Code = 0;
                            resp.Description = "Cliente Vinculado al Rol, satisfactoriamente!";
                            break;

                        case 1: // Desvincular
                            strSQL = "update us_roles set cod_empresa = null where cod_rol = @CodRol";
                            connection.Execute(strSQL, new { req.CodRol });
                            resp.Code = 1;
                            resp.Description = "Cliente Desvinculado al Rol, satisfactoriamente!";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            //return resp;
        }

        public List<RolesObtenerDto> RolFiltroObtener(string filtro)
        {
            List<RolesObtenerDto> resp = new List<RolesObtenerDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var strSQL = "SELECT R.cod_Rol, R.descripcion, R.activo, " +
                        "CONVERT(varchar(10), ISNULL(R.cod_Empresa, 0)) + '- ' + RTRIM(ISNULL(C.Nombre_Largo, 'General')) AS 'Cliente', " +
                        "R.registro_Fecha, R.registro_Usuario " +
                        "FROM US_Roles R " +
                        "LEFT JOIN PGX_Clientes C ON R.cod_empresa = C.cod_Empresa " +
                        $"WHERE R.descripcion LIKE '%{filtro}%' " +
                        "ORDER BY R.descripcion";

                    return connection.Query<RolesObtenerDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        public ErrorDto RolGuardar(RolInsertarDto rol)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                int activo = rol.Activo == true ? 1 : 0;
                int cliente = Convert.ToInt16(rol.Cliente.Substring(0, 1));

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    connection.Open();

                    var existe = connection.QueryFirstOrDefault<int>($@"SELECT ISNULL(COUNT(*), 0) FROM US_Roles WHERE Cod_Rol = '{rol.Cod_Rol}'");

                    if (existe == 0) // Insertar
                    {
                        var strSQL = $@"INSERT INTO US_Roles (Cod_Rol, Descripcion, Activo, Registro_Fecha, Registro_Usuario) 
                                        VALUES ('{rol.Cod_Rol.ToUpper()}', '{rol.Descripcion}', 1, GETDATE(), '{rol.Registro_Usuario}')";

                        connection.Execute(strSQL);

                        resp.Description = "Insercion Exitosa!";
                    }
                    else // Actualizar
                    {

                        var strSQL = $@"UPDATE US_Roles SET Descripcion = '{rol.Descripcion}', 
                                       Activo = {activo} WHERE Cod_Rol = '{rol.Cod_Rol}'";

                        connection.Execute(strSQL);

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

        public ErrorDto RolEliminar(string CodRol)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var strSQL = "DELETE FROM US_Roles WHERE Cod_Rol = @CodRol";
                    connection.Execute(strSQL, new { CodRol });

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

        public List<RolesObtenerDto> RolesObtener()
        {
            List<RolesObtenerDto> resp = new List<RolesObtenerDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                resp = null;
            }
            return resp;
        }

        public List<ClientesObtenerDto> ClientesObtener()
        {
            List<ClientesObtenerDto> resp = new List<ClientesObtenerDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var strSQL = "SELECT [COD_EMPRESA]"
                              + ",[NOMBRE_LARGO]"
                              + ",[NOMBRE_CORTO]"
                          + "FROM[PGX_Portal].[dbo].[PGX_CLIENTES]";

                    resp = connection.Query<ClientesObtenerDto>(strSQL).ToList();
                }

                resp.Insert(0, new ClientesObtenerDto { Cod_Empresa = "0", Nombre_Largo = "General", Nombre_Corto = "General" });
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

    }
}
