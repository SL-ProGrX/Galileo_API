using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsUsuariosDb
    {

        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsUsuariosDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<ExplorerRootInfoDto> ExplorerRootInfoObtener(int codEmpresa)
        {
            try
            {
                var connectionString = new SqlConnectionStringBuilder(
                    _config.GetConnectionString(connectionStringName));
                return DbHelper.CreateOkResponse(new ExplorerRootInfoDto
                {
                    Servidor = connectionString.DataSource,
                    BaseDatos = connectionString.InitialCatalog,
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<ExplorerRootInfoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Valida si existe el usuario en el sistema
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<int> UsuarioExiste(string usuario)
        {
            var resp = DbHelper.CreateOkResponse(0);

            string sql = "select count(*) as 'Existe' from US_USUARIOS where Usuario = @usuario";
            var values = new
            {
                usuario = usuario,
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Result = connection.Query<int>(sql, values).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }

            return resp;
        }


        /// <summary>
        /// Guarda o Actualiza el usuario según el modo de edición
        /// </summary>
        /// <param name="usuarioDto"></param>
        /// <returns></returns>
        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioDto)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var procedure = "[spPGX_W_Usuario_Guardar_Actualizar]";

                    var parameters = new DynamicParameters();

                    parameters.Add("@Usuario", usuarioDto.UserName);
                    parameters.Add("@Identificacion", usuarioDto.Identificacion);
                    parameters.Add("@Nombre", usuarioDto.Nombre);
                    parameters.Add("@Notas", usuarioDto.Notas);
                    parameters.Add("@EMail", usuarioDto.Email);
                    parameters.Add("@TelCelular", usuarioDto.TelCelular);
                    parameters.Add("@TelTrabajo", usuarioDto.TelTrabajo);
                    parameters.Add("@ContabilizaCobranza", usuarioDto.ContabilizaCobranza);
                    parameters.Add("@UsuarioRegistro", usuarioDto.UsuarioRegistro);
                    parameters.Add("@UserId", usuarioDto.UserId, DbType.Int32, ParameterDirection.InputOutput);
                    parameters.Add("@ModoEdicion", (usuarioDto.ModoEdicion ?? false) ? 1 : 0);
                    parameters.Add("@EmpresaId", usuarioDto.EmpresaId);
                    parameters.Add("@NombreEmpresa", usuarioDto.NombreEmpresa);
                    parameters.Add("@AppVersion", string.Empty);
                    parameters.Add("@AppName", "SSECURITY- WEB");
                    parameters.Add("@Maquina", string.Empty);
                    parameters.Add("@MACAdress", string.Empty);
                    parameters.Add("@Tfa_ind", (usuarioDto.tfa_ind ?? false) ? 1 : 0);
                    parameters.Add("@Tfa_metodo", usuarioDto.tfa_metodo);
                    parameters.Add("@EsAdminPortal", dbType: DbType.Boolean, direction: ParameterDirection.Output);


                    // Execute the stored procedure
                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output values
                    var esAdminPortal = parameters.Get<bool>("@EsAdminPortal");


                    if (!esAdminPortal && !(usuarioDto.ModoEdicion ?? false) && usuarioDto.EmpresaId > 0)
                    {
                        try
                        {
                            SincronizaUsuarioCore(usuarioDto.EmpresaId, usuarioDto.UserName, usuarioDto.Nombre, "A", usuarioDto.UsuarioRegistro);
                        }
                        catch (Exception)
                        {
                            throw new InvalidOperationException("Se presento un problema al sincronizar el usuario con el Core");
                        }
                    }

                    resp.Description = "Ok";

                }
            }
            catch (SqlException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Consulta un usuario 
        /// </summary>
        /// <param name="paramUsuario"></param>
        /// <param name="codEmpresa"></param>
        /// <param name="AdminView"></param>
        /// <param name="DirGlobal"></param>
        /// <returns></returns>
        public ErrorDto<UsuarioModel?> UsuarioConsultar(string paramUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            UsuarioModel? result = null;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Usuario_Consultar]";
                    var values = new
                    {
                        Usuario = paramUsuario,
                        Empresa = codEmpresa,
                        AdminView = AdminView,
                        DirGlobal = DirGlobal
                    };
                    result = connection.QueryFirstOrDefault<UsuarioModel>(procedure, values, commandType: CommandType.StoredProcedure);

                    if (result != null && result.FechaIngreso != null)
                    {
                        DateTime? dFechaIngreso = result.FechaIngreso.Value.Date;
                        result.FechaIngreso = dFechaIngreso;
                    }

                    if (result != null && result.FechaUltimo != null)
                    {
                        DateTime? dFechaUltimo = result.FechaUltimo.Value.Date;
                        result.FechaUltimo = dFechaUltimo;
                    }
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<UsuarioModel?>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }


        public ErrorDto<List<UsuarioModel>> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            List<UsuarioModel>? result = null;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Usuarios_Empresa_Obtener]";
                    var values = new
                    {
                        Empresa = codEmpresa,
                        AdminView = AdminView,
                        DirGlobal = DirGlobal
                    };
                    result = connection.Query<UsuarioModel>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioModel>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }

        public ErrorDto<List<UsuarioModel>> UsuariosExplorerObtener(int codEmpresa)
        {
            List<UsuarioModel>? result = null;
            try
            {
                if (codEmpresa <= 0)
                    return DbHelper.CreateErrorResponse<List<UsuarioModel>>("La empresa es requerida.");

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                const string sql = """
                    SELECT
                        U.Usuario AS UserName,
                        U.UserID AS UserId,
                        U.Nombre AS Nombre,
                        CASE WHEN U.ESTADO = 'A' THEN 'Activo' ELSE 'Inactivo' END AS Estado,
                        U.Registro_Fecha AS FechaIngreso,
                        U.Fecha_Mod AS FechaUltimo,
                        U.Contabiliza AS ContabilizaCobranza
                    FROM US_USUARIOS U
                    INNER JOIN PGX_CLIENTES_USERS C ON C.USUARIO = U.USUARIO
                        AND C.COD_EMPRESA = @CodEmpresa
                    ORDER BY U.NOMBRE
                    """;
                result = connection.Query<UsuarioModel>(sql, new { CodEmpresa = codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioModel>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }

        public ErrorDto<List<UsuarioClienteDto>> UsuarioClientesConsultar(string nombreUsuario)
        {
            List<UsuarioClienteDto>? result = null;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Clientes]";
                    var values = new
                    {
                        Usuario = nombreUsuario
                    };
                    result = connection.Query<UsuarioClienteDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioClienteDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }

        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                string valEstado = usuarioClienteAsignaDto.Estado.Trim();
                string valNota = string.Empty;

                if (valEstado == "I")
                {
                    valNota = "Membresía al Rol: (" + usuarioClienteAsignaDto.CodigoEmpresa.ToString() + " ) " + usuarioClienteAsignaDto.NombreEmpresa;
                }
                else if (valEstado == "E")
                {
                    valNota = "Exclusión al Rol: ( " + usuarioClienteAsignaDto.CodigoEmpresa.ToString() + " ) " + usuarioClienteAsignaDto.NombreEmpresa;
                }

                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    try
                    {
                        int res;

                        var procedure = "[spPGX_Usuario_Cliente_Asigna]";
                        var values = new
                        {
                            Cliente = usuarioClienteAsignaDto.CodigoEmpresa,
                            Usuario = usuarioClienteAsignaDto.Usuario,
                            UsuarioRegistra = usuarioClienteAsignaDto.UsuarioRegistra,
                            TipoMov = usuarioClienteAsignaDto.Estado,
                            Notas = string.Empty
                        };

                        res = connection.Execute(procedure, values, /*transaction,*/ commandType: CommandType.StoredProcedure);

                        if (res > 0)
                        {
                            var procedureSegLog = "[spSEG_Log]";

                            var valuesSegLog = new
                            {
                                AppName = usuarioClienteAsignaDto.AppName,
                                AppVersion = usuarioClienteAsignaDto.AppVersion,
                                Usuario = usuarioClienteAsignaDto.Usuario,
                                Transac = "08",
                                Notas = valNota,
                                UserMov = usuarioClienteAsignaDto.UsuarioRegistra,
                                Equipo = string.Empty,
                                EquipoMAC = string.Empty
                            };

                            res = connection.Execute(procedureSegLog, valuesSegLog, /*transaction,*/ commandType: CommandType.StoredProcedure);
                        }

                        if (res > 0)
                        {
                            if (usuarioClienteAsignaDto.Estado == "I")
                            {
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa ?? 0, usuarioClienteAsignaDto.Usuario, "", "A", usuarioClienteAsignaDto.UsuarioRegistra);
                            }
                            else
                            {
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa ?? 0, usuarioClienteAsignaDto.Usuario, "", "I", usuarioClienteAsignaDto.UsuarioRegistra);
                            }
                        }

                        if (res > 0)
                        {
                            resp.Code = 0;
                            resp.Description = string.Empty;
                        }
                        else
                        {
                            resp.Code = -1;
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
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<TipoTransaccionBitacora>> UsuarioCuentaTiposTransaccionObtener()
        {
            List<TipoTransaccionBitacora> resultado;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = $@"select cod_transac as 'Codigo' , rtrim(descripcion) as Descripcion from us_transacciones";
                    resultado = connection.Query<TipoTransaccionBitacora>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TipoTransaccionBitacora>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(resultado);
        }

        public ErrorDto<List<UsuarioCuentaBitacora>> UsuarioBitacoraConsultar(UsuarioBitacoraRequest request)
        {
            List<UsuarioCuentaBitacora>? result = null;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Bitacora]";
                    var values = new
                    {
                        Usuario = request.Usuario,
                        Lineas = request.Lineas,
                        FechaInicio = request.FechaInicio,
                        FechaCorte = request.FechaCorte,
                        CodTransac = request.CodTransac
                    };
                    result = connection.Query<UsuarioCuentaBitacora>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioCuentaBitacora>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }

        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            List<UsuarioClienteRolDto> result = new();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Roles]";
                    var values = new
                    {
                        Usuario = nombreUsuario,
                        CodEmpresa = codEmpresa
                    };
                    // El procedimiento devuelve CodRol como entero, mientras el
                    // contrato HTTP lo expone como texto para conservar el
                    // formato usado por SSecurity Web.
                    static object? Valor(IDataRecord row, string nombre)
                    {
                        var esperado = nombre.Replace("_", string.Empty, StringComparison.Ordinal)
                            .ToLowerInvariant();
                        for (var index = 0; index < row.FieldCount; index++)
                        {
                            var actual = row.GetName(index).Replace("_", string.Empty, StringComparison.Ordinal)
                                .ToLowerInvariant();
                            if (actual == esperado)
                                return row.IsDBNull(index) ? null : row.GetValue(index);
                        }
                        return null;
                    }

                    using var reader = connection.ExecuteReader(procedure, values, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        result.Add(new UsuarioClienteRolDto
                        {
                            CodigoRol = Convert.ToString(Valor(reader, "CodigoRol")) ?? string.Empty,
                            Descripcion = Convert.ToString(Valor(reader, "Descripcion")) ?? string.Empty,
                            Asignado = Convert.ToBoolean(Valor(reader, "Asignado")),
                            RegistroFecha = Convert.ToDateTime(Valor(reader, "RegistroFecha")),
                            RegistroUsuario = Convert.ToString(Valor(reader, "RegistroUsuario")) ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioClienteRolDto>>(ex.Message);
            }
            return DbHelper.CreateOkResponse(result);
        }

        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesExplorerObtener(string nombreUsuario, int codEmpresa)
        {
            try
            {
                const string sql = @"
                    SELECT
                        M.cod_Rol AS CodigoRol,
                        R.Descripcion,
                        CAST(1 AS bit) AS Asignado,
                        M.registro_Fecha AS RegistroFecha,
                        CAST('' AS nvarchar(100)) AS RegistroUsuario,
                        C.Nombre_Largo AS ClienteLink
                    FROM US_ROL_MIEMBROS M
                    INNER JOIN US_ROLES R ON R.cod_Rol = M.cod_Rol
                    INNER JOIN PGX_CLIENTES C ON C.cod_Empresa = M.cod_Empresa
                    WHERE M.Usuario = @NombreUsuario
                      AND (@CodEmpresa = 0 OR M.cod_Empresa = @CodEmpresa)
                    ORDER BY C.Nombre_Largo, R.Descripcion";

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var result = connection.Query<UsuarioClienteRolDto>(sql, new { NombreUsuario = nombreUsuario, CodEmpresa = codEmpresa }).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<UsuarioClienteRolDto>>(ex.Message);
            }
        }

        public ErrorDto<List<RolMiembroExplorerDto>> RolMiembrosExplorerObtener(string rolId, int codEmpresa)
        {
            try
            {
                const string sql = @"
                    SELECT
                        U.Usuario,
                        U.Nombre,
                        M.registro_Fecha AS RegistroFecha,
                        CASE WHEN U.Estado = 'A' THEN 'Activo' ELSE 'Inactivo' END AS Estado,
                        C.Nombre_Largo AS ClienteLink
                    FROM US_ROL_MIEMBROS M
                    INNER JOIN US_USUARIOS U ON U.Usuario = M.Usuario
                    LEFT JOIN PGX_CLIENTES C ON C.cod_Empresa = M.cod_Empresa
                    WHERE M.cod_Rol = @RolId
                      AND (@CodEmpresa = 0 OR M.cod_Empresa = @CodEmpresa)
                    ORDER BY C.Nombre_Largo, U.Nombre, M.registro_Fecha";

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var result = connection.Query<RolMiembroExplorerDto>(sql, new { RolId = rolId, CodEmpresa = codEmpresa }).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<RolMiembroExplorerDto>>(ex.Message);
            }
        }

        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Usuario_Rol_Asigna]";
                    var values = new
                    {
                        Cliente = usuarioClienteRolAsignaDto.CodigoEmpresa,
                        Usuario = usuarioClienteRolAsignaDto.Usuario,
                        Rol = usuarioClienteRolAsignaDto.CodigoRol,
                        UsuarioRegistra = usuarioClienteRolAsignaDto.UsuarioRegistra,
                        TipoMov = usuarioClienteRolAsignaDto.Estado
                    };
                    resp.Code = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                    resp.Description = "Ok";
                    resp.Description = string.Empty;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public int SincronizaUsuarioCore(int pCodEmpresa, string pUsuario, string pNombre, string pEstado, string pUsrLogon)
        {
            int res = -1;

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(pCodEmpresa);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var procedure = "[spSEG_SincronizaUsuarios]";

                var sincroUsuarioCore = new
                {
                    Usuario = pUsuario,
                    Nombre = pNombre,
                    Estado = pEstado,
                    RegUser = pUsrLogon
                };
                res = connection.Execute(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Se presento un problema al sincronizar el usuario con el Core: " + ex.Message);
            }
            return res;
        }

    }
}
