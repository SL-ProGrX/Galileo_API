using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
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
                    EmpresaId = (long)(dto.Cliente ?? 0),
                    Us_Origen = dto.UsBase,
                    Us_Destino = dto.UsDestino,
                    Usuario = dto.Usuario,
                    Copy_Rol = (dto.RS_Roles ?? false) ? 1 : 0,
                    Copy_Estacion = (dto.RS_Estaciones ?? false) ? 1 : 0,
                    Copy_Horario = (dto.RS_Horarios ?? false) ? 1 : 0,
                    Inicializa = (dto.RS_Inicializa ?? false) ? 1 : 0
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

            // Validación temprana: sin try/catch ni else
            if (!dto.Cliente.HasValue)
            {
                errorMsg = "El valor de Cliente no puede ser nulo.";
                return -1;
            }

            try
            {
                string stringConn = new PortalDB(_config)
                    .ObtenerDbConnStringEmpresa(dto.Cliente.Value);

                using var connectionCliente = new SqlConnection(stringConn);

                var copiaAccesosCore = CrearCopiaAccesosCore(dto);

                return connectionCliente.Query<int>(
                        "spSys_Users_Copy_Roles",
                        copiaAccesosCore,
                        commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception exCore)
            {
                errorMsg = "Hubo un problema al sincronizar accesos de usuario en el Core: "
                           + exCore.Message;
                return -1;
            }
        }

        private static object CrearCopiaAccesosCore(UsuarioPermisosCopiar dto)
        {
            return new
            {
                Us_Destino = dto.UsDestino,
                Us_Origen = dto.UsBase,
                Usuairo = dto.Usuario.ToUpper(),   // deja el nombre tal cual si el SP lo espera así
                R_Oficina = 1,
                R_Deducciones = BoolToInt(dto.RO_Deducciones),
                R_Contabilidad = BoolToInt(dto.RO_Contabilidad),
                R_Gestion_Crd = BoolToInt(dto.RO_Creditos),
                R_Resolucion_Crd = BoolToInt(dto.RO_Resolucion_Crd),
                R_Cobros = BoolToInt(dto.RO_Cobros),
                R_Cajas = BoolToInt(dto.RO_Cajas),
                R_Bancos = BoolToInt(dto.RO_Bancos),
                R_Presupuesto = BoolToInt(dto.RO_Presupuesto),
                R_Inventario = BoolToInt(dto.RO_Inventarios),
                R_Compras = BoolToInt(dto.RO_Compras),
                R_Inicializa = BoolToInt(dto.RO_Inicializa)
            };
        }

        private static int BoolToInt(bool? value) => value == true ? 1 : 0;

        private void RegistrarBitacoraCopia(UsuarioPermisosCopiar dto)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = (long)(dto.Cliente ?? 0),
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
