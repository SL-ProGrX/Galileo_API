using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposProcesos;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposProcesosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovimientoRegistro = "REGISTRA - WEB";
        private const string MovimientoModificacion = "MODIFICA - WEB";
        private const string MovimientoEliminacion = "ELIMINAR - WEB";
        private const int ModuloBitacora = 40;

        private const string ColumnaCodigo = "CodTipoProceso";
        private const string ColumnaNombre = "NombreTipoProceso";
        private const string ColumnaActivo = "Activo";

        private const string MensajeDatosInvalidos = "Datos requeridos.";
        private const string MensajeUsuarioInvalido = "El usuario es requerido.";
        private const string MensajeCodigoInvalido = "El campo 'CodTipoProceso' es requerido.";
        private const string MensajeGuardarFallido = "No fue posible guardar el tipo de proceso.";
        private const string MensajeEliminarFallido = "No fue posible eliminar el tipo de proceso.";

        /// <summary>
        /// El filtro se aplica sobre el código y nombre del tipo de proceso, usando LIKE para permitir búsquedas parciales.
        /// </summary>
        private const string FiltroSql = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoProceso AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoProceso LIKE @like)";

        /// <summary>
        /// La consulta de conteo usa el mismo filtro que la consulta de datos para asegurar que el total corresponda al número de registros que cumplen la condición de búsqueda.
        /// </summary>
        private const string ConteoSql = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + FiltroSql + ";";

        /// <summary>
        /// La consulta base selecciona los campos necesarios para el listado y aplica el filtro. El ordenamiento y paginación se agregan dinámicamente según los parámetros recibidos.
        /// </summary>
        private const string ConsultaBaseSql = @"
            SELECT
                CodTipoProceso,
                NombreTipoProceso,
                Activo
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + FiltroSql;

        /// <summary>
        /// El comando de guardado primero intenta actualizar el registro existente con el código proporcionado. Si no existe (es decir, @@ROWCOUNT = 0), entonces inserta un nuevo registro. Esto permite manejar tanto la creación como la modificación con una sola operación, simplificando la lógica en la capa de aplicación.
        /// </summary>
        private const string GuardarSql = @"
            DECLARE @accion NVARCHAR(10);

            UPDATE dbo.AFI_CD_TIPO_PROCESO
            SET
                NombreTipoProceso = @NombreTipoProceso,
                Activo = @Activo,
                Modifica_Fecha = dbo.MyGetdate(),
                Modifica_Usuario = @usuario
            WHERE CodTipoProceso = @CodTipoProceso;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.AFI_CD_TIPO_PROCESO
                (
                    CodTipoProceso,
                    NombreTipoProceso,
                    Activo,
                    RegistroFecha,
                    RegistroUsuario
                )
                VALUES
                (
                    UPPER(@CodTipoProceso),
                    @NombreTipoProceso,
                    @Activo,
                    dbo.MyGetdate(),
                    @usuario
                );

                SET @accion = N'insert';
            END
            ELSE
            BEGIN
                SET @accion = N'update';
            END

            SELECT @accion AS accion;";

        /// <summary>
        /// El comando de eliminación borra el registro con el código especificado. Se asume que no existen restricciones de integridad referencial que impidan la eliminación, o que dichas restricciones se manejan a nivel de base de datos (por ejemplo, con ON DELETE CASCADE) o en la lógica de aplicación antes de llamar a este método.
        /// </summary>
        private const string EliminarSql = @"
            DELETE FROM dbo.AFI_CD_TIPO_PROCESO
            WHERE CodTipoProceso = @CodTipoProceso;";

        /// <summary>
        ///  Inicializa el acceso a base de datos y seguridad.
        /// </summary>
        /// <param name="config"></param>
        public FrmAfCdTiposProcesosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        ///  Devuelve la lista con filtro, orden y paginación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CdTiposProcesosLista> AfCdTiposProcesosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var filtroTexto = filtros.filtro?.Trim();
                var usaFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
                var campoOrden = GetSortColumn(filtros.sortField);
                var direccion = filtros.sortOrder == 1 ? "DESC" : "ASC";

                var parametros = new
                {
                    filtro = usaFiltro ? filtroTexto : null,
                    like = usaFiltro ? $"%{filtroTexto}%" : null,
                    orderBy = campoOrden,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                };

                var total = conn.QuerySingle<int>(ConteoSql, parametros);
                var sql = ArmarConsultaListado(direccion, filtros.paginacion > 0 && !esExportar);
                var datos = conn.Query<CdTiposProcesosData>(sql, parametros).ToList();

                return new CdTiposProcesosLista
                {
                    Total = total,
                    lista = datos
                };
            });
        }

        /// <summary>
        ///  Inserta o actualiza el tipo de proceso.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, CdTiposProcesosData datos)
        {
            if (datos == null)
            {
                return DbHelper.ErrorResponse(MensajeDatosInvalidos);
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoProceso))
            {
                return DbHelper.ErrorResponse(MensajeCodigoInvalido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(MensajeUsuarioInvalido);
            }

            var codigo = datos.CodTipoProceso.Trim();
            var ejecucion = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                GuardarSql,
                defaultValue: string.Empty,
                parameters: new
                {
                    CodTipoProceso = codigo,
                    datos.NombreTipoProceso,
                    datos.Activo,
                    usuario
                });

            if (ejecucion.Code != 0)
            {
                return DbHelper.ErrorResponse(MensajeGuardarFallido);
            }

            var accion = (ejecucion.Result ?? string.Empty).Trim().ToLowerInvariant();
            if (accion == "insert")
            {
                EscribirBitacora(codEmpresa, usuario, codigo, MovimientoRegistro);
            }
            else if (accion == "update")
            {
                EscribirBitacora(codEmpresa, usuario, codigo, MovimientoModificacion);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codTipoProceso"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposProcesos_Eliminar(int codEmpresa, string usuario, string codTipoProceso)
        {
            if (string.IsNullOrWhiteSpace(codTipoProceso))
            {
                return DbHelper.ErrorResponse(MensajeCodigoInvalido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(MensajeUsuarioInvalido);
            }

            var codigo = codTipoProceso.Trim();
            var ejecucion = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                EliminarSql,
                new { CodTipoProceso = codigo });

            if (ejecucion.Code != 0)
            {
                return DbHelper.ErrorResponse(MensajeEliminarFallido);
            }

            if (ejecucion.Result > 0)
            {
                EscribirBitacora(codEmpresa, usuario, codigo, MovimientoEliminacion);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        ///  Registra el detalle del movimiento en bitácora.
        /// </summary>
        /// <param name="empresaId"></param>
        /// <param name="usuario"></param>
        /// <param name="codigo"></param>
        /// <param name="movimiento"></param>
        private void EscribirBitacora(int empresaId, string usuario, string codigo, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = $"Tipos de Proceso Id: {codigo}",
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        /// <summary>
        ///  Resuelve la columna válida para ordenar.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static string GetSortColumn(string? sortField)
        {
            var valor = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            return valor switch
            {
                "codtipoproceso" => ColumnaCodigo,
                "nombretipoproceso" => ColumnaNombre,
                "activo" => ColumnaActivo,
                _ => ColumnaCodigo
            };
        }

        /// <summary>
        ///  Construye el SQL final del listado.
        /// </summary>
        /// <param name="direccion"></param>
        /// <param name="usarPaginacion"></param>
        /// <returns></returns>
        private static string ArmarConsultaListado(string direccion, bool usarPaginacion)
        {
            var sql = ConsultaBaseSql + $@"
            ORDER BY
                CASE WHEN @orderBy = '{ColumnaCodigo}' THEN CodTipoProceso END {direccion},
                CASE WHEN @orderBy = '{ColumnaNombre}' THEN NombreTipoProceso END {direccion},
                CASE WHEN @orderBy = '{ColumnaActivo}' THEN Activo END {direccion}";

            if (usarPaginacion)
            {
                sql += " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sql += ";";
            }

            return sql;
        }
    }
}