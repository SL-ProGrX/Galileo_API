using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfPerfilTransaccionalDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;

        public FrmAfPerfilTransaccionalDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de perfiles transaccionales según filtros y ordenamiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de paginación, búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de perfiles transaccionales</returns>
        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( " +
                    "CAST(PT_Id AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Monto_Minimo AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Monto_Maximo AS VARCHAR) LIKE @Filtro OR " +
                    "Nivel LIKE @Filtro OR " +
                    "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE @Filtro OR " +
                    "Registro_Usuario LIKE @Filtro ) ";
            }

            string sortField = string.IsNullOrEmpty(filtros?.sortField) ? "Monto_Minimo" : filtros.sortField;
            string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";
            int pagina = filtros?.pagina ?? 0;
            int paginacion = filtros?.paginacion ?? 10;

            string queryTotal = "SELECT COUNT(PT_Id) FROM AFI_PERFIL_TRANSACCIONAL" + where;
            string queryLista = $@"SELECT PT_Id, Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_PERFIL_TRANSACCIONAL
                               {where}
                               ORDER BY {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryTotal, 0, parametros);
            var lista = DbHelper.ExecuteListQuery<PerfilTransaccionalData>(_portalDb, codEmpresa, queryLista, parametros);

            return new ErrorDto<PerfilTransaccionalLista>
            {
                Code = 0,
                Description = "OK",
                Result = new PerfilTransaccionalLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un perfil transaccional según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="perfil">Datos del perfil transaccional</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_PerfilTransaccional_Guardar(int codEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            string queryExiste = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PERFIL_TRANSACCIONAL WHERE PT_Id = @PT_Id";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { perfil.PT_Id });
            ErrorDto result;
            if (existe.Result == 0)
            {
                result = AF_PerfilTransaccional_Insertar(codEmpresa, usuario, perfil);
            }
            else
            {
                result = AF_PerfilTransaccional_Actualizar(codEmpresa, usuario, perfil);
            }
            return result;
        }

        /// <summary>
        /// Inserta un nuevo perfil transaccional en la base de datos.
        /// </summary>
        private ErrorDto AF_PerfilTransaccional_Insertar(int codEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            string query = @"INSERT INTO AFI_PERFIL_TRANSACCIONAL
                              (Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario)
                              VALUES (@Monto_Minimo, @Monto_Maximo, @Nivel, @Activo, GETDATE(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                perfil.Monto_Minimo,
                perfil.Monto_Maximo,
                perfil.Nivel,
                Activo = perfil.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Perfil Transaccional : {perfil.Nivel} ({perfil.Monto_Minimo}-{perfil.Monto_Maximo})");
            }
            return result;
        }

        /// <summary>
        /// Actualiza un perfil transaccional existente en la base de datos.
        /// </summary>
        private ErrorDto AF_PerfilTransaccional_Actualizar(int codEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            string query = @"UPDATE AFI_PERFIL_TRANSACCIONAL
                              SET Monto_Minimo = @Monto_Minimo,
                                  Monto_Maximo = @Monto_Maximo,
                                  Nivel = @Nivel,
                                  Activo = @Activo,
                                  Modifica_Fecha = GETDATE(),
                                  Modifica_Usuario = @Usuario
                              WHERE PT_Id = @PT_Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                perfil.PT_Id,
                perfil.Monto_Minimo,
                perfil.Monto_Maximo,
                perfil.Nivel,
                Activo = perfil.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Perfil Transaccional : {perfil.Nivel} ({perfil.Monto_Minimo}-{perfil.Monto_Maximo})");
            }
            return result;
        }

        /// <summary>
        /// Elimina un perfil transaccional por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="ptId">Identificador del perfil transaccional</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_PerfilTransaccional_Eliminar(int codEmpresa, string usuario, int ptId)
        {
            string query = "DELETE FROM AFI_PERFIL_TRANSACCIONAL WHERE PT_Id = @PT_Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { PT_Id = ptId });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Perfil Transaccional : {ptId}");
            }
            return result;
        }

        /// <summary>
        /// Exporta la lista de perfiles transaccionales según filtros (sin paginación).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de perfiles transaccionales</returns>
        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Exportar(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( " +
                    "CAST(PT_Id AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Monto_Minimo AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Monto_Maximo AS VARCHAR) LIKE @Filtro OR " +
                    "Nivel LIKE @Filtro OR " +
                    "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE @Filtro OR " +
                    "Registro_Usuario LIKE @Filtro ) ";
            }
            string query = $@"SELECT PT_Id, Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_PERFIL_TRANSACCIONAL
                               {where}
                               ORDER BY Monto_Minimo ASC";
            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var lista = DbHelper.ExecuteListQuery<PerfilTransaccionalData>(_portalDb, codEmpresa, query, parametros);
            return new ErrorDto<PerfilTransaccionalLista>
            {
                Code = 0,
                Description = "OK",
                Result = new PerfilTransaccionalLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
