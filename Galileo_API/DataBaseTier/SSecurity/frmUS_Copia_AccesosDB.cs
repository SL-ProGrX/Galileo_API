using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_Copia_AccesosDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;

        public frmUS_Copia_AccesosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Lista de usuarios de una empresa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public List<UsuarioEmpresa> UsuariosEmpresa_Obtener(int codEmpresa)
        {
            List<UsuarioEmpresa> info = new List<UsuarioEmpresa>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var query = $@"SELECT usuario,nombre FROM vPGX_Usuarios_Empresa WHERE cod_Empresa = {codEmpresa}";

                    info = connection.Query<UsuarioEmpresa>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info!;
        }

        /// <summary>
        /// Copia accesos de un usuario a otro
        /// </summary>
        /// <param name="copiaPermisosUsuarioDto"></param>
        /// <returns></returns>
        public ErrorDto UsuarioAccesos_Copiar(UsuarioPermisosCopiar copiaPermisosUsuarioDto)
        {
            ErrorDto resultado = new ErrorDto();
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    try
                    {
                        var copiaAccesos = new
                        {
                            EmpresaId = copiaPermisosUsuarioDto.Cliente,
                            Us_Origen = copiaPermisosUsuarioDto.UsBase,
                            Us_Destino = copiaPermisosUsuarioDto.UsDestino,
                            Usuario = copiaPermisosUsuarioDto.Usuario,
                            Copy_Rol = copiaPermisosUsuarioDto.RS_Roles ? 1 : 0,
                            Copy_Estacion = copiaPermisosUsuarioDto.RS_Estaciones ? 1 : 0,
                            Copy_Horario = copiaPermisosUsuarioDto.RS_Horarios ? 1 : 0,
                            Inicializa = copiaPermisosUsuarioDto.RS_Inicializa ? 1 : 0
                        };

                        //resultado.Code = connection.Execute("spSEG_Copia_Permisos", copiaAccesos, commandType: CommandType.StoredProcedure);
                        resultado.Code = connection.Query<int>("spSEG_Copia_Permisos", copiaAccesos, commandType: CommandType.StoredProcedure).FirstOrDefault();

                        if (resultado.Code == 0)
                        {
                            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(copiaPermisosUsuarioDto.Cliente);

                            using var connectionCliente = new SqlConnection(stringConn);
                            {
                                try
                                {
                                    var copiaAccesosCore = new
                                    {
                                        Us_Destino = copiaPermisosUsuarioDto.UsDestino,
                                        Us_Origen = copiaPermisosUsuarioDto.UsBase,
                                        Usuairo = copiaPermisosUsuarioDto.Usuario.ToUpper(),
                                        R_Oficina = 1,
                                        R_Deducciones = copiaPermisosUsuarioDto.RO_Deducciones ? 1 : 0,
                                        R_Contabilidad = copiaPermisosUsuarioDto.RO_Contabilidad ? 1 : 0,
                                        R_Gestion_Crd = copiaPermisosUsuarioDto.RO_Creditos ? 1 : 0,
                                        R_Resolucion_Crd = copiaPermisosUsuarioDto.RO_Resolucion_Crd ? 1 : 0,
                                        R_Cobros = copiaPermisosUsuarioDto.RO_Cobros ? 1 : 0,
                                        R_Cajas = copiaPermisosUsuarioDto.RO_Cajas ? 1 : 0,
                                        R_Bancos = copiaPermisosUsuarioDto.RO_Bancos ? 1 : 0,
                                        R_Presupuesto = copiaPermisosUsuarioDto.RO_Presupuesto ? 1 : 0,
                                        R_Inventario = copiaPermisosUsuarioDto.RO_Inventarios ? 1 : 0,
                                        R_Compras = copiaPermisosUsuarioDto.RO_Compras ? 1 : 0,
                                        R_Inicializa = copiaPermisosUsuarioDto.RO_Inicializa ? 1 : 0
                                    };

                                    //resultado.Code = connectionCliente.Execute("spSys_Users_Copy_Roles", copiaAccesosCore,commandType: CommandType.StoredProcedure);
                                    resultado.Code = connectionCliente.Query<int>("spSys_Users_Copy_Roles", copiaAccesosCore, commandType: CommandType.StoredProcedure).FirstOrDefault();
                                    resultado.Description = "Ok";

                                    if (resultado.Code == 0)
                                    {
                                        Bitacora(new BitacoraInsertarDto
                                        {
                                            EmpresaId = copiaPermisosUsuarioDto.Cliente,
                                            Usuario = copiaPermisosUsuarioDto.Usuario,
                                            DetalleMovimiento = "Copia de Permisos Empresa [" + copiaPermisosUsuarioDto.Cliente + "] del Usuario " + copiaPermisosUsuarioDto.UsBase + " -> " + copiaPermisosUsuarioDto.UsDestino,
                                            Movimiento = "APLICA - WEB",
                                            Modulo = 13
                                        });
                                    }
                                }
                                catch (Exception exCore)
                                {
                                    throw new Exception("Hubo un problema al sincronizar accesos de usuario en el Core: " + exCore.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        resultado.Code = -1;
                        resultado.Description = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            finally
            {
                seguridadPortal = null!;
            }
            return resultado;
        }

        /// <summary>
        /// Consulta el usuario de una empresa
        /// </summary>
        /// <param name="nombreUsuario"></param>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public UsuarioEmpresa UsuarioEmpresa_Obtener(string nombreUsuario, int codEmpresa)
        {
            UsuarioEmpresa result = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Usuario_Empresa_Consultar]";
                    var values = new
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = nombreUsuario,
                    };
                    result = connection.QueryFirstOrDefault<UsuarioEmpresa>(procedure, values, commandType: CommandType.StoredProcedure)!;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

    }
}
