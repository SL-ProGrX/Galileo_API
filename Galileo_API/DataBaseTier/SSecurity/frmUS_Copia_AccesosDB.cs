using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class FrmUsCopiaAccesosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";


        readonly MSecurityMainDb DBBitacora;

        public FrmUsCopiaAccesosDb(IConfiguration config)
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
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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
            var resultado = new ErrorDto();

            try
            {
                resultado.Code = CopiarPermisos(copiaPermisosUsuarioDto, out var errorMsg);

                if (resultado.Code == 0)
                {
                    resultado.Code = CopiarRolesCore(copiaPermisosUsuarioDto, out var coreErrorMsg);

                    if (resultado.Code == 0)
                    {
                        RegistrarBitacoraCopia(copiaPermisosUsuarioDto);
                        resultado.Description = "Ok";
                    }
                    else
                    {
                        resultado.Description = coreErrorMsg;
                    }
                }
                else
                {
                    resultado.Description = errorMsg;
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }

            return resultado;
        }

        private int CopiarPermisos(UsuarioPermisosCopiar dto, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                var copiaAccesos = new
                {
                    EmpresaId = dto.Cliente,
                    Us_Origen = dto.UsBase,
                    Us_Destino = dto.UsDestino,
                    Usuario = dto.Usuario,
                    Copy_Rol = dto.RS_Roles ? 1 : 0,
                    Copy_Estacion = dto.RS_Estaciones ? 1 : 0,
                    Copy_Horario = dto.RS_Horarios ? 1 : 0,
                    Inicializa = dto.RS_Inicializa ? 1 : 0
                };

                return connection.Query<int>("spSEG_Copia_Permisos", copiaAccesos, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return -1;
            }
        }

        private int CopiarRolesCore(UsuarioPermisosCopiar dto, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(dto.Cliente);
                using var connectionCliente = new SqlConnection(stringConn);

                var copiaAccesosCore = new
                {
                    Us_Destino = dto.UsDestino,
                    Us_Origen = dto.UsBase,
                    Usuairo = dto.Usuario.ToUpper(),
                    R_Oficina = 1,
                    R_Deducciones = dto.RO_Deducciones ? 1 : 0,
                    R_Contabilidad = dto.RO_Contabilidad ? 1 : 0,
                    R_Gestion_Crd = dto.RO_Creditos ? 1 : 0,
                    R_Resolucion_Crd = dto.RO_Resolucion_Crd ? 1 : 0,
                    R_Cobros = dto.RO_Cobros ? 1 : 0,
                    R_Cajas = dto.RO_Cajas ? 1 : 0,
                    R_Bancos = dto.RO_Bancos ? 1 : 0,
                    R_Presupuesto = dto.RO_Presupuesto ? 1 : 0,
                    R_Inventario = dto.RO_Inventarios ? 1 : 0,
                    R_Compras = dto.RO_Compras ? 1 : 0,
                    R_Inicializa = dto.RO_Inicializa ? 1 : 0
                };

                return connectionCliente.Query<int>("spSys_Users_Copy_Roles", copiaAccesosCore, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception exCore)
            {
                errorMsg = "Hubo un problema al sincronizar accesos de usuario en el Core: " + exCore.Message;
                return -1;
            }
        }

        private void RegistrarBitacoraCopia(UsuarioPermisosCopiar dto)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = dto.Cliente,
                Usuario = dto.Usuario,
                DetalleMovimiento = $"Copia de Permisos Empresa [{dto.Cliente}] del Usuario {dto.UsBase} -> {dto.UsDestino}",
                Movimiento = "APLICA - WEB",
                Modulo = 13
            });
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
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
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
