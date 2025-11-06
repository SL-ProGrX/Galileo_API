using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class FrmUsUsuariosDb
    {

        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsUsuariosDb(IConfiguration config)
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

            string sql = "select count(*) as 'Existe' from US_USUARIOS where Usuario = @usuario";
            var values = new
            {
                usuario = usuario,
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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


                    // Execute the stored procedure
                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output values
                    var esAdminPortal = parameters.Get<bool>("@EsAdminPortal");


                    if (esAdminPortal && !usuarioDto.ModoEdicion && usuarioDto.EmpresaId > 0)
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
        public UsuarioModel UsuarioConsultar(string paramUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            UsuarioModel result = null!;
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
                    result = connection.QueryFirstOrDefault<UsuarioModel>(procedure, values, commandType: CommandType.StoredProcedure)!;

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







        public List<UsuarioModel> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            List<UsuarioModel> result = null!;
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
                    result = connection.Query<UsuarioModel>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result!;
        }



        public List<UsuarioClienteDto> UsuarioClientesConsultar(string nombreUsuario)
        {
            List<UsuarioClienteDto> result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Usuario_Consultar_Clientes]";
                    var values = new
                    {
                        Usuario = nombreUsuario
                    };
                    result = connection.Query<UsuarioClienteDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
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
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa, usuarioClienteAsignaDto.Usuario, "", "A", usuarioClienteAsignaDto.UsuarioRegistra);
                            }
                            else
                            {
                                res = SincronizaUsuarioCore(usuarioClienteAsignaDto.CodigoEmpresa, usuarioClienteAsignaDto.Usuario, "", "I", usuarioClienteAsignaDto.UsuarioRegistra);
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

        public List<TipoTransaccionBitacora> UsuarioCuentaTiposTransaccionObtener()
        {
            List<TipoTransaccionBitacora> resultado = new List<TipoTransaccionBitacora>();
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
                _ = ex.Message;
            }
            return resultado!;
        }

        public List<UsuarioCuentaBitacora> UsuarioBitacoraConsultar(UsuarioBitacoraRequest request)
        {
            List<UsuarioCuentaBitacora> result = null!;
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
                    result = connection.Query<UsuarioCuentaBitacora>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public List<UsuarioClienteRolDto> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            List<UsuarioClienteRolDto> result = null!;
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
                    result = connection.Query<UsuarioClienteRolDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Se presento un problema al sincronizar el usuario con el Core: " + ex.Message);
            }
            return res;
        }

    }


}//end namespace
