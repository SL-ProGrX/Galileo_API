using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.US;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_UsuariosDB
    {

        private readonly IConfiguration _config;

        public frmUS_UsuariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Valida si existe el usuario en el sistema
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public int UsuarioExiste(string usuario)
        {
            int resp = 0;
            // string stringConn = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

            string sql = "select count(*) as 'Existe' from US_USUARIOS where Usuario = @usuario";
            var values = new
            {
                usuario = usuario,
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    resp = connection.Query<int>(sql, values).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Guarda o Actualiza el usuario según el modo de edición
        /// </summary>
        /// <param name="usuarioDto"></param>
        /// <returns></returns>
        public ErrorDto UsuarioGuardarActualizar(UsuarioDTO usuarioDto)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    parameters.Add("@ModoEdicion", usuarioDto.ModoEdicion ? 1 : 0);
                    parameters.Add("@EmpresaId", usuarioDto.EmpresaId);
                    parameters.Add("@NombreEmpresa", usuarioDto.NombreEmpresa);
                    parameters.Add("@AppVersion", string.Empty);
                    parameters.Add("@AppName", "SSECURITY- WEB");
                    parameters.Add("@Maquina", string.Empty);
                    parameters.Add("@MACAdress", string.Empty);
                    parameters.Add("@Tfa_ind", usuarioDto.tfa_ind ? 1 : 0);
                    parameters.Add("@Tfa_metodo", usuarioDto.tfa_metodo);
                    parameters.Add("@EsAdminPortal", dbType: DbType.Boolean, direction: ParameterDirection.Output);



                    //resp.Code = connection.Query<int>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();



                    // Execute the stored procedure
                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output values
                    var esAdminPortal = parameters.Get<bool>("@EsAdminPortal");
                    var userIdOutput = parameters.Get<int?>("@UserId");


                    if (esAdminPortal && !usuarioDto.ModoEdicion && usuarioDto.EmpresaId > 0)
                    {
                        try
                        {
                            int res = SincronizaUsuarioCore(usuarioDto.EmpresaId, usuarioDto.UserName, usuarioDto.Nombre, "A", usuarioDto.UsuarioRegistro);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Se presento un problema al sincronizar el usuario con el Core");
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
        public UsuarioDTO UsuarioConsultar(string paramUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            UsuarioDTO result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Usuario_Consultar]";
                    var values = new
                    {
                        Usuario = paramUsuario,
                        Empresa = codEmpresa,
                        AdminView = AdminView,
                        DirGlobal = DirGlobal
                    };
                    result = connection.QueryFirstOrDefault<UsuarioDTO>(procedure, values, commandType: CommandType.StoredProcedure)!;

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
                _ = ex.Message;
            }
            return result!;
        }







        public List<UsuarioDTO> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            List<UsuarioDTO> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Usuarios_Empresa_Obtener]";
                    var values = new
                    {
                        Empresa = codEmpresa,
                        AdminView = AdminView,
                        DirGlobal = DirGlobal
                    };
                    result = connection.Query<UsuarioDTO>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result!;
        }



        public List<UsuarioClienteDTO> UsuarioClientesConsultar(string nombreUsuario)
        {
            List<UsuarioClienteDTO> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Clientes]";
                    var values = new
                    {
                        Usuario = nombreUsuario
                    };
                    result = connection.Query<UsuarioClienteDTO>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDTO usuarioClienteAsignaDto)
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

                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    //connection.Open();

                    //using (SqlTransaction transaction = connection.BeginTransaction()) //Consultar si desean manejar la transaccionalidad?
                    //{
                    try
                    {
                        int res;

                        //sqlCommand = "exec spPGX_Usuario_Cliente_Asigna @Cliente, @Usuario, @UsuarioRegistra, @TipoMov, @Notas";
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
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa, usuarioClienteAsignaDto.Usuario, "", "A", usuarioClienteAsignaDto.UsuarioRegistra);
                            }
                            else
                            {
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa, usuarioClienteAsignaDto.Usuario, "", "I", usuarioClienteAsignaDto.UsuarioRegistra);
                            }
                        }

                        if (res > 0)
                        {
                            //transaction.Commit();
                            resp.Code = 0;
                            resp.Description = string.Empty;
                        }
                        else
                        {
                            //transaction.Rollback();
                            resp.Code = -1;
                        }

                    }
                    catch (Exception)
                    {
                        //Console.WriteLine($"Error: {ex.Message}");

                        // Rollback de la transacción en caso de error.
                        //transaction.Rollback();
                        throw;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public List<TipoTransaccionBitacora> UsuarioCuentaTiposTransaccionObtener()
        {
            List<TipoTransaccionBitacora> resultado = new List<TipoTransaccionBitacora>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var query = $@"select cod_transac as 'Codigo' , rtrim(descripcion) as Descripcion from us_transacciones";
                    resultado = connection.Query<TipoTransaccionBitacora>(query).ToList();
                    //var procedure = "[spPGX_Cuenta_Log_Transacciones_Obtener]";

                    //resultado = connection.Query<TipoTransaccionBitacora>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado!;
        }

        public List<UsuarioCuentaBitacora> UsuarioBitacoraConsultar(UsuarioBitacoraRequest request)
        {
            List<UsuarioCuentaBitacora> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    result = connection.Query<UsuarioCuentaBitacora>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<UsuarioClienteRolDTO> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            List<UsuarioClienteRolDTO> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Roles]";
                    var values = new
                    {
                        Usuario = nombreUsuario,
                        CodEmpresa = codEmpresa
                    };
                    result = connection.Query<UsuarioClienteRolDTO>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDTO usuarioClienteRolAsignaDto)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);
            int res = -1;

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(pCodEmpresa);
            try
            {

                //pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(pCodEmpresa);
                //string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
                //string nombreBDCore = pgxClienteDto.PGX_CORE_DB;
                //string userId = pgxClienteDto.PGX_CORE_USER;
                //string pass = pgxClienteDto.PGX_CORE_KEY;

                //string connectionString = $"Data Source={nombreServidorCore};" +
                //                      $"Initial Catalog={nombreBDCore};" +
                //                      $"Integrated Security=False;User Id={userId};Password={pass};";

                /*string connectionString = $"PROVIDER=MSDASQL;Driver={{SQL Server}};Server={server};" +
                      $"Database=PGX_BASE;APP=PGX_Portal;tcp:{server};";

                string connectionString = $"PROVIDER=MSDASQL;Driver={{SQL Server}};Server={nombreServidorCore};" +
                                        $"Database={nombreBDCore};APP=PGX_Portal;tcp:{nombreServidorCore};";*/

                using var connection = new SqlConnection(clienteConnString);
                {
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
            }
            catch
            {
                throw;
            }
            finally
            {
                seguridadPortal = null!;
            }
            return res;
        }

    }


}//end namespace
