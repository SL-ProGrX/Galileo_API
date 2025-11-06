using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class FrmUsRolesMembresiasDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsRolesMembresiasDb(IConfiguration config)
        {
            _config = config;
        }


        public List<UsuariosConsultaDto> UsuariosConsultar(string? usuario, bool adminView, bool dirGlobal, int codEmpresa)
        {
            List<UsuariosConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select Usuario,Nombre,UserID"
                               + " from US_Usuarios"
                               + " where Estado = 'A' and (Usuario like '%" + usuario + "%' or Nombre like '%" + usuario + "%')";
                    if (!adminView)
                    {
                        strSQL = strSQL + " AND isnull(key_admin,0) = 0";
                    }
                    else if (!dirGlobal)
                    {
                        //Solo Usuarios que han formado parte de este cliente anteriormente, si por error fue desvinculado
                        strSQL = strSQL + " AND usuario in(select usuario from PGX_CLIENTES_USERS_H"
                                + " Where cod_Empresa = " + codEmpresa + ")";
                    }

                    resp = connection.Query<UsuariosConsultaDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<UsuariosConsultaDto>();
            }
            return resp;
        }

        public List<UsuariosVinculadosConsultaDto> UsuariosVinculadosConsultar(string? usuario, bool contabiliza, bool adminView, int codEmpresa)
        {
            List<UsuariosVinculadosConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select U.Usuario,U.Nombre,U.UserID,A.registro_Fecha,A.Registro_Usuario"
                                   + " from US_Usuarios U inner join PGX_Clientes_USERS A on U.Usuario = A.usuario and A.cod_Empresa = " + codEmpresa
                                   + " where (U.Usuario like '%" + usuario + "%' or U.Nombre like '%" + usuario + "%') and U.Contabiliza = " + Convert.ToInt16(contabiliza);
                    if (!adminView)
                    {
                        strSQL = strSQL + " AND isnull(key_admin,0) = 0";
                    }
                    strSQL = strSQL + " order by U.Nombre";


                    resp = connection.Query<UsuariosVinculadosConsultaDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<UsuariosVinculadosConsultaDto>();
            }
            return resp;
        }

        public Limites Limites_Obtener(string usuario, int codEmpresa)
        {
            Limites info = new Limites();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = @"SELECT ISNULL(Limita_Acceso_Estacion, 0) AS Estacion, 
                                  ISNULL(Limita_Acceso_Horario, 0) AS Horario
                           FROM PGX_Clientes_Users 
                           WHERE cod_empresa = @CodEmpresa 
                           AND usuario = @Usuario";

                    var parameters = new
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = usuario
                    };

                    info = connection.QueryFirstOrDefault<Limites>(strSQL, parameters) ?? new Limites();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<RolConsultaDto> RolesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            List<RolConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = $@"SELECT R.COD_ROL, R.DESCRIPCION, 
                                   CASE WHEN ISNULL(M.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END AS Asignado,
                                   M.REGISTRO_FECHA, M.REGISTRO_USUARIO
                            FROM US_ROLES R
                            LEFT JOIN US_ROL_MIEMBROS M 
                            ON R.COD_ROL = M.COD_ROL 
                            AND M.COD_EMPRESA = @CodEmpresa
                            AND M.USUARIO = @Usuario
                            WHERE R.ACTIVO = 1 
                            AND ISNULL(R.COD_EMPRESA, @CodEmpresa) = @CodEmpresa ";

                    // Append the DESCRIPCION filter only if 'filtro' is not empty
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        strSQL += " AND R.DESCRIPCION LIKE '%' + @Filtro + '%' ";
                    }

                    strSQL += @"ORDER BY CASE WHEN ISNULL(M.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END DESC, 
                                R.DESCRIPCION ASC";

                    // Define the parameters
                    var parameters = new
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = usuario,
                        Filtro = filtro // Will be ignored if empty
                    };

                    // Execute the query
                    resp = connection.Query<RolConsultaDto>(strSQL, parameters).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<RolConsultaDto>();
            }
            return resp;
        }

        public List<HorarioConsultaDto> HorariosConsultar(string usuario, string? filtro, int codEmpresa)
        {
            List<HorarioConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = @"SELECT E.COD_HORARIO, E.DESCRIPCION, 
                                  CASE WHEN ISNULL(A.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END AS Asignado,
                                  A.REGISTRO_FECHA, A.REGISTRO_USUARIO
                           FROM PGX_CLIENTES_HORARIOS E
                           LEFT JOIN PGX_CLIENTES_HORARIOS_USERS A 
                           ON E.COD_HORARIO = A.COD_HORARIO 
                           AND E.COD_EMPRESA = @CodEmpresa
                           AND A.USUARIO = @Usuario
                           WHERE E.ACTIVO = 1 
                           AND ISNULL(E.COD_EMPRESA, @CodEmpresa) = @CodEmpresa ";

                    // Append the DESCRIPCION filter only if 'filtro' is not empty
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        strSQL += " AND E.DESCRIPCION LIKE '%' + @Filtro + '%' ";
                    }

                    strSQL += @"ORDER BY CASE WHEN ISNULL(A.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END DESC, 
                               E.DESCRIPCION ASC";

                    // Define the parameters
                    var parameters = new
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = usuario,
                        Filtro = filtro // Will be ignored if empty
                    };

                    // Execute the query
                    resp = connection.Query<HorarioConsultaDto>(strSQL, parameters).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<HorarioConsultaDto>();
            }
            return resp;
        }

        public List<EstacionConsultaDto> EstacionesConsultar(string usuario, string? filtro, int codEmpresa)
        {
            List<EstacionConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = @"SELECT E.ESTACION, E.DESCRIPCION, 
                                  CASE WHEN ISNULL(A.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END AS Asignado,
                                  A.REGISTRO_FECHA, A.REGISTRO_USUARIO
                           FROM PGX_CLIENTES_ESTACIONES E
                           LEFT JOIN PGX_CLIENTES_ESTACIONES_USERS A 
                           ON E.ESTACION = A.ESTACION 
                           AND E.COD_EMPRESA = @CodEmpresa
                           AND A.USUARIO = @Usuario
                           WHERE E.ACTIVA = 1 
                           AND ISNULL(E.COD_EMPRESA, @CodEmpresa) = @CodEmpresa ";

                    // Append the DESCRIPCION filter only if 'filtro' is not empty
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        strSQL += " AND E.DESCRIPCION LIKE '%' + @Filtro + '%' ";
                    }

                    strSQL += @"ORDER BY CASE WHEN ISNULL(A.REGISTRO_USUARIO, '') = '' THEN 0 ELSE 1 END DESC, 
                               E.DESCRIPCION ASC";

                    // Define the parameters
                    var parameters = new
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = usuario,
                        Filtro = filtro // Will be ignored if empty
                    };

                    // Execute the query
                    resp = connection.Query<EstacionConsultaDto>(strSQL, parameters).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<EstacionConsultaDto>();
            }
            return resp;
        }




        public ErrorDto UsuarioClienteAsigna(UsuarioClienteAsigna req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var values = new
                    {
                        Cliente = req.Cliente,
                        Usuario = req.Usuario,
                        UsuarioRegistra = req.UsuarioRegistra,
                        TipoMov = req.TipoMov,
                        Notas = ""
                    };

                    resp.Code = connection.QueryFirstOrDefault<int>("spPGX_Usuario_Cliente_Asigna", values, commandType: CommandType.StoredProcedure);
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto UsuarioRolAsigna(UsuarioRolAsignaDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.QueryFirstOrDefault<int>("spPGX_Usuario_Rol_Asigna", req, commandType: CommandType.StoredProcedure);
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Acceso_Equipo(EstacionAsignaDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.Query<int>("spPGX_Usuario_Estacion_Asigna", req, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Limita_Equipo(LimitaAcceso req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.Query<int>("spPGX_Usuario_Estacion_Limita", req, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }



        public ErrorDto Acceso_Horario(HorarioAsignaDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.Query<int>("spPGX_Usuario_Horario_Asigna", req, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Limita_Horario(LimitaAcceso req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.Query<int>("spPGX_Usuario_Horario_Limita", req, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
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
