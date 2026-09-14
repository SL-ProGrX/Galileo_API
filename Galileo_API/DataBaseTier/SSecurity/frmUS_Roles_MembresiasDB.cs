using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsRolesMembresiasDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsRolesMembresiasDb(IConfiguration config)
        {
            _config = config;
        }
        private ErrorDto<T> QuerySingleResponse<T>(string sql, object? parameters = null)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                return DbHelper.CreateOkResponse(connection.QueryFirstOrDefault<T>(sql, parameters)!);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<T>(ex.Message);
            }
        }


        public ErrorDto<List<UsuariosConsultaDto>> UsuariosConsultar(string? usuario, bool adminView, bool dirGlobal, int codEmpresa)
        {
            List<UsuariosConsultaDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select Usuario,Nombre,UserID"
                                + " from US_Usuarios"
                                + " where Estado = 'A' and (Usuario like @usuarioPattern or Nombre like @usuarioPattern)";
                    if (!adminView)
                    {
                        strSQL = strSQL + " AND isnull(key_admin,0) = 0";
                    }
                    else if (!dirGlobal)
                    {
                        //Solo Usuarios que han formado parte de este cliente anteriormente, si por error fue desvinculado
                        strSQL = strSQL + " AND usuario in (select usuario from PGX_CLIENTES_USERS_H Where cod_Empresa = @codEmpresa)";
                    }

                    var parameters = new
                    {
                        usuarioPattern = usuario != null ? $"%{usuario}%" : "%",
                        codEmpresa = codEmpresa
                    };

                    resp = connection.Query<UsuariosConsultaDto>(strSQL, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuariosConsultaDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(resp);
        }

        public ErrorDto<List<UsuariosVinculadosConsultaDto>> UsuariosVinculadosConsultar(string? usuario, int contabiliza, bool adminView, int codEmpresa)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var sql = @"
                    SELECT U.Usuario, U.Nombre, U.UserID, A.registro_Fecha, A.Registro_Usuario
                    FROM US_Usuarios U
                    INNER JOIN PGX_Clientes_USERS A 
                        ON U.Usuario = A.usuario AND A.cod_Empresa = @codEmpresa
                    WHERE (U.Usuario LIKE @usuarioPattern OR U.Nombre LIKE @usuarioPattern)
                ";

                if (contabiliza != 2)
                    sql += " AND U.Contabiliza = @contabiliza";

                if (!adminView)
                    sql += " AND ISNULL(key_admin,0) = 0";

                sql += " ORDER BY U.Nombre";

                var parameters = new
                {
                    codEmpresa,
                    usuarioPattern = "%" + (usuario ?? "") + "%",
                    contabiliza
                };

                return DbHelper.CreateOkResponse(connection.Query<UsuariosVinculadosConsultaDto>(sql, parameters).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuariosVinculadosConsultaDto>>(ex.Message);
            }
        }


        public ErrorDto<Limites> Limites_Obtener(string usuario, int codEmpresa)
        {
            const string sql = @"SELECT ISNULL(Limita_Acceso_Estacion, 0) AS Estacion,
                                        ISNULL(Limita_Acceso_Horario, 0) AS Horario
                                 FROM PGX_Clientes_Users
                                 WHERE cod_empresa = @CodEmpresa
                                   AND usuario = @Usuario";

            var response = QuerySingleResponse<Limites>(sql, new
            {
                CodEmpresa = codEmpresa,
                Usuario = usuario
            });

            response.Result ??= new Limites();
            return response;
        }

        public ErrorDto<List<RolConsultaDto>> RolesConsultar(string usuario, string? filtro, int codEmpresa)
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
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<RolConsultaDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(resp);
        }

        public ErrorDto<List<HorarioConsultaDto>> HorariosConsultar(string usuario, string? filtro, int codEmpresa)
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
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<HorarioConsultaDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(resp);
        }

        public ErrorDto<List<EstacionConsultaDto>> EstacionesConsultar(string usuario, string? filtro, int codEmpresa)
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
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<EstacionConsultaDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(resp);
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
                    if (resp.Code != 0)
                    {
                        resp.Description = "No fue posible actualizar la membresía del usuario.";
                        return resp;
                    }

                    var estadoCore = req.TipoMov == 'I' ? "A" : "I";
                    var coreResult = SincronizaUsuarioCore(
                        req.Cliente ?? 0,
                        req.Usuario,
                        req.UsuarioNombre,
                        estadoCore,
                        req.UsuarioRegistra);

                    if (coreResult < 0)
                    {
                        resp.Code = -1;
                        resp.Description = "La membresía se actualizó, pero no fue posible sincronizar el usuario con el Core.";
                        return resp;
                    }

                    resp.Description = "Membresía actualizada correctamente.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private int SincronizaUsuarioCore(int codEmpresa, string usuario, string nombre, string estado, string usuarioRegistra)
        {
            if (codEmpresa <= 0 || string.IsNullOrWhiteSpace(usuario))
            {
                return -1;
            }

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            using var connection = new SqlConnection(clienteConnString);
            connection.Open();

            var parameters = new
            {
                Usuario = usuario,
                Nombre = nombre ?? string.Empty,
                Estado = estado,
                RegUser = usuarioRegistra ?? string.Empty
            };

            return connection.Execute("spSEG_SincronizaUsuarios", parameters, commandType: CommandType.StoredProcedure);
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
