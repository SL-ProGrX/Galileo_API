using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfNoCotizaRangosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;

        public FrmAfNoCotizaRangosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de rangos de no cotiza según filtros y ordenamiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de paginación, búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de rangos</returns>
        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( " +
                    "CAST(Linea_Id AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Dia_Desde AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Dia_Hasta AS VARCHAR) LIKE @Filtro OR " +
                    "Descripcion LIKE @Filtro OR " +
                    "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE @Filtro OR " +
                    "Registro_Usuario LIKE @Filtro ) ";
            }

            string sortField = string.IsNullOrEmpty(filtros?.sortField) ? "Dia_Desde" : filtros.sortField;
            string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";
            int pagina = filtros?.pagina ?? 0;
            int paginacion = filtros?.paginacion ?? 10;

            string queryTotal = "SELECT COUNT(Linea_Id) FROM AFI_SOCIOS_SIN_APORTES_RANGOS" + where;
            string queryLista = $@"SELECT Linea_Id, Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_SOCIOS_SIN_APORTES_RANGOS
                               {where}
                               ORDER BY {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryTotal, 0, parametros);
            var lista = DbHelper.ExecuteListQuery<NoCotizaRangosData>(_portalDb, codEmpresa, queryLista, parametros);

            return new ErrorDto<NoCotizaRangosLista>
            {
                Code = 0,
                Description = "OK",
                Result = new NoCotizaRangosLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un rango de no cotiza según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="rango">Datos del rango</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_NoCotizaRangos_Guardar(int codEmpresa, string usuario, NoCotizaRangosData rango)
        {
            string queryExiste = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_SOCIOS_SIN_APORTES_RANGOS WHERE Linea_Id = @Linea_Id";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { rango.Linea_Id });
            return existe.Result == 0
                ? AF_NoCotizaRangos_Insertar(codEmpresa, usuario, rango)
                : AF_NoCotizaRangos_Actualizar(codEmpresa, usuario, rango);
        }

        /// <summary>
        /// Inserta un nuevo rango de no cotiza en la base de datos.
        /// </summary>
        private ErrorDto AF_NoCotizaRangos_Insertar(int codEmpresa, string usuario, NoCotizaRangosData rango)
        {
            string query = @"INSERT INTO AFI_SOCIOS_SIN_APORTES_RANGOS
                              (Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario)
                              VALUES (@Dia_Desde, @Dia_Hasta, @Descripcion, @Activo, GETDATE(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                rango.Dia_Desde,
                rango.Dia_Hasta,
                rango.Descripcion,
                Activo = rango.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"No Cotiza Rango : {rango.Descripcion} ({rango.Dia_Desde}-{rango.Dia_Hasta})");
            }
            return result;
        }

        /// <summary>
        /// Actualiza un rango de no cotiza existente en la base de datos.
        /// </summary>
        private ErrorDto AF_NoCotizaRangos_Actualizar(int codEmpresa, string usuario, NoCotizaRangosData rango)
        {
            string query = @"UPDATE AFI_SOCIOS_SIN_APORTES_RANGOS
                              SET Dia_Desde = @Dia_Desde,
                                  Dia_Hasta = @Dia_Hasta,
                                  Descripcion = @Descripcion,
                                  Activo = @Activo,
                                  Modifica_Fecha = GETDATE(),
                                  Modifica_Usuario = @Usuario
                              WHERE Linea_Id = @Linea_Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                rango.Linea_Id,
                rango.Dia_Desde,
                rango.Dia_Hasta,
                rango.Descripcion,
                Activo = rango.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"No Cotiza Rango : {rango.Descripcion} ({rango.Dia_Desde}-{rango.Dia_Hasta})");
            }
            return result;
        }

        /// <summary>
        /// Elimina un rango de no cotiza por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="lineaId">Identificador del rango</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_NoCotizaRangos_Eliminar(int codEmpresa, string usuario, int lineaId)
        {
            string query = "DELETE FROM AFI_SOCIOS_SIN_APORTES_RANGOS WHERE Linea_Id = @Linea_Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { Linea_Id = lineaId });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"No Cotiza Rango : {lineaId}");
            }
            return result;
        }

        /// <summary>
        /// Exporta la lista de rangos de no cotiza según filtros (sin paginación).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de rangos</returns>
        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Exportar(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE ( " +
                    "CAST(Linea_Id AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Dia_Desde AS VARCHAR) LIKE @Filtro OR " +
                    "CAST(Dia_Hasta AS VARCHAR) LIKE @Filtro OR " +
                    "Descripcion LIKE @Filtro OR " +
                    "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE @Filtro OR " +
                    "Registro_Usuario LIKE @Filtro ) ";
            }
            string query = $@"SELECT Linea_Id, Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_SOCIOS_SIN_APORTES_RANGOS
                               {where}
                               ORDER BY Dia_Desde ASC";
            var parametros = (filtros != null && !string.IsNullOrEmpty(filtros.filtro)) ? new { Filtro = "%" + filtros.filtro + "%" } : null;
            var lista = DbHelper.ExecuteListQuery<NoCotizaRangosData>(_portalDb, codEmpresa, query, parametros);
            return new ErrorDto<NoCotizaRangosLista>
            {
                Code = 0,
                Description = "OK",
                Result = new NoCotizaRangosLista
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
