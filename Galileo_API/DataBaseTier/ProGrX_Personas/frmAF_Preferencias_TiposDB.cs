using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfPreferenciasTiposDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;

        public FrmAfPreferenciasTiposDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de preferencias tipos según filtros y ordenamiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de paginación, búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de preferencias tipos</returns>
        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( COD_PREFERENCIA LIKE @Filtro OR descripcion LIKE @Filtro OR Registro_Usuario LIKE @Filtro ) ";
            }

            string sortField = string.IsNullOrEmpty(filtros?.sortField) ? "COD_PREFERENCIA" : filtros.sortField;
            string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";
            int pagina = filtros?.pagina ?? 0;
            int paginacion = filtros?.paginacion ?? 10;

            string queryTotal = "SELECT COUNT(COD_PREFERENCIA) FROM AFI_PREFERENCIAS" + where;
            string queryLista = $@"SELECT COD_PREFERENCIA, descripcion, ACTIVA, Registro_Fecha, Registro_Usuario
                                   FROM AFI_PREFERENCIAS
                                   {where}
                                   ORDER BY {sortField} {sortOrder}
                                   OFFSET {pagina} ROWS FETCH NEXT {paginacion} ROWS ONLY";

            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryTotal, 0, parametros);
            var lista = DbHelper.ExecuteListQuery<PreferenciaTipoData>(_portalDb, codEmpresa, queryLista, parametros);

            return new ErrorDto<PreferenciaTipoLista>
            {
                Code = 0,
                Description = "OK",
                Result = new PreferenciaTipoLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de preferencia según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="preferenciaTipo">Datos del tipo de preferencia</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_Preferencias_Guardar(int codEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            string queryExiste = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { COD_PREFERENCIA = preferenciaTipo.Cod_Preferencia });
            return existe.Result == 0
                 ? AF_Preferencias_Insertar(codEmpresa, usuario, preferenciaTipo)
                 : AF_Preferencias_Actualizar(codEmpresa, usuario, preferenciaTipo);
        }

        /// <summary>
        /// Inserta un nuevo tipo de preferencia en la base de datos.
        /// </summary>
        private ErrorDto AF_Preferencias_Insertar(int codEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            string query = @"INSERT INTO AFI_PREFERENCIAS (Cod_Preferencia, Descripcion, Activa, Registro_Fecha, Registro_Usuario)
                              VALUES (@Cod_Preferencia, @Descripcion, @Activa, GETDATE(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                preferenciaTipo.Cod_Preferencia,
                preferenciaTipo.Descripcion,
                Activa = preferenciaTipo.Activa ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Tipo de Preferencia : {preferenciaTipo.Cod_Preferencia} - {preferenciaTipo.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Actualiza un tipo de preferencia existente en la base de datos.
        /// </summary>
        private ErrorDto AF_Preferencias_Actualizar(int codEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            string query = @"UPDATE AFI_PREFERENCIAS
                              SET Descripcion = @Descripcion,
                                  Activa = @Activa
                              WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                preferenciaTipo.Cod_Preferencia,
                preferenciaTipo.Descripcion,
                Activa = preferenciaTipo.Activa ? 1 : 0
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Tipo de Preferencia : {preferenciaTipo.Cod_Preferencia} - {preferenciaTipo.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Elimina un tipo de preferencia por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="codPreferencia">Código del tipo de preferencia</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_Preferencias_Eliminar(int codEmpresa, string usuario, string codPreferencia)
        {
            string query = "DELETE FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { COD_PREFERENCIA = codPreferencia });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Tipo de Preferencia : {codPreferencia}");
            }
            return result;
        }

        /// <summary>
        /// Valida si una preferencia ya existe por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codPreferencia">Código de la preferencia</param>
        /// <returns>ErrorDto indicando si la preferencia existe o es válida</returns>
        public ErrorDto AF_Preferencias_Valida(int codEmpresa, string codPreferencia)
        {
            var result = new ErrorDto();
            string query = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { COD_PREFERENCIA = codPreferencia }).Result;
            if (existe > 0)
            {
                result.Code = -1;
                result.Description = "La preferencia ya existe.";
            }
            else
            {
                result.Code = 0;
                result.Description = "La preferencia es válida.";
            }
            return result;
        }

        /// <summary>
        /// Exporta la lista de preferencias según filtros (sin paginación).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de preferencias</returns>
        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Exportar(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( COD_PREFERENCIA LIKE @Filtro OR descripcion LIKE @Filtro OR Registro_Usuario LIKE @Filtro ) ";
            }
            string query = $@"SELECT COD_PREFERENCIA, descripcion, ACTIVA, Registro_Fecha, Registro_Usuario
                               FROM AFI_PREFERENCIAS
                               {where}
                               ORDER BY COD_PREFERENCIA";
            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var lista = DbHelper.ExecuteListQuery<PreferenciaTipoData>(_portalDb, codEmpresa, query, parametros);
            return new ErrorDto<PreferenciaTipoLista>
            {
                Code = 0,
                Description = "OK",
                Result = new PreferenciaTipoLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
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
