using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaAsientosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXUtilEliminaAsientosDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXUtilEliminaAsientosDb(
            PortalDB portalDb,
            MSecurityMainDb mSecurityMainDb)
        {
            (_portalDb, _mSecurityMainDb) = (portalDb, mSecurityMainDb);
        }

        /// <summary>
        /// Obtiene los tipos de asiento configurados para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad activa.</param>
        /// <returns>Lista de tipos de asiento.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(
            int codEmpresa,
            int cod_contabilidad)
        {
            const string sql = @"SELECT tipo_asiento as item,
                        RTRIM(tipo_asiento) + ' - ' + RTRIM(descripcion) AS descripcion
                          FROM CntX_Tipos_Asientos
                          WHERE cod_contabilidad = @cod_contabilidad";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, sql, new { cod_contabilidad });
        }

        /// <summary>
        /// Calcula los asientos pendientes de eliminar dentro del rango indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad activa.</param>
        /// <param name="tipo_asiento">Tipo de asiento seleccionado.</param>
        /// <param name="desde">Número de asiento inicial.</param>
        /// <param name="hasta">Número de asiento final.</param>
        /// <param name="anio">Año del período contable.</param>
        /// <param name="mes">Mes del período contable.</param>
        /// <returns>Total de asientos pendientes que cumplen los filtros.</returns>
        public ErrorDto<int> Cntx_Util_Asientos_Calcular(int codEmpresa, int cod_contabilidad, string tipo_asiento,
                string desde, string hasta, int anio, int mes)
        {
            const string sql = @"SELECT COUNT(*)
          FROM Cntx_Asientos
          WHERE anio = @anio
          AND mes = @mes
          AND tipo_asiento = @tipo_asiento
          AND num_asiento BETWEEN @desde AND @hasta
          AND fecha_aplicado IS NULL
          AND cod_contabilidad = @cod_contabilidad
          AND modulo = 20";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sql, 0,
                new { cod_contabilidad, tipo_asiento, desde, hasta, anio, mes });
        }

        /// <summary>
        /// Elimina, en una transacción, los encabezados elegibles y sus detalles.
        /// </summary>
        /// <param name="request">Empresa, contabilidad, período, tipo y rango de números de asiento.</param>
        /// <returns>Indicador de finalización o el error producido.</returns>
        public ErrorDto<bool> Cntx_Util_Asientos_Eliminar(CntxEliminarAsientosRequestDto request)
        {
            if (!request.cod_empresa.HasValue || !request.cod_contabilidad.HasValue ||
                !request.anio.HasValue || !request.mes.HasValue ||
                string.IsNullOrWhiteSpace(request.tipo_asiento) ||
                string.IsNullOrWhiteSpace(request.desde) || string.IsNullOrWhiteSpace(request.hasta))
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "La empresa, contabilidad, período, tipo y rango de asientos son obligatorios.");
            }

            const string eliminarDetallesSql = @"
                DELETE detalle
                FROM Cntx_Asientos_detalle detalle
                WHERE EXISTS (
                    SELECT 1
                    FROM Cntx_Asientos asiento
                    WHERE asiento.num_asiento = detalle.num_asiento
                      AND asiento.tipo_asiento = detalle.tipo_asiento
                      AND asiento.cod_contabilidad = detalle.cod_contabilidad
                      AND asiento.tipo_asiento = @tipo_asiento
                      AND asiento.num_asiento BETWEEN @desde AND @hasta
                      AND asiento.cod_contabilidad = @cod_contabilidad
                      AND asiento.fecha_aplicado IS NULL
                      AND asiento.anio = @anio
                      AND asiento.mes = @mes
                      AND asiento.modulo = 20);";

            const string eliminarEncabezadosSql = @"
                DELETE Cntx_Asientos
                WHERE tipo_asiento = @tipo_asiento
                  AND num_asiento BETWEEN @desde AND @hasta
                  AND cod_contabilidad = @cod_contabilidad
                  AND fecha_aplicado IS NULL
                  AND anio = @anio
                  AND mes = @mes
                  AND modulo = 20;";

            var response = DbHelper.WithConn(_portalDb, request.cod_empresa.Value, cn =>
            {
                cn.Open();
                using var transaction = cn.BeginTransaction();
                try
                {
                    cn.Execute(eliminarDetallesSql, request, transaction);
                    cn.Execute(eliminarEncabezadosSql, request, transaction);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            if (response.Code == 0)
            {
                try
                {
                    _mSecurityMainDb.Bitacora(
                        new Galileo.Models.Security.BitacoraInsertarDto
                        {
                            EmpresaId = request.cod_empresa.Value,
                            Usuario = request.usuario!,
                            Movimiento = "Elimina Asientos - WEB",
                            DetalleMovimiento =
                                $"TIPO:{request.tipo_asiento} D:{request.desde} H:{request.hasta}",
                            Modulo = 20
                        });
                    response.Description = "Eliminación finalizada.";
                }
                catch (Exception ex)
                {
                    return DbHelper.CreateErrorResponse<bool>(
                        $"Los asientos se eliminaron, pero no se pudo registrar la bitácora: {ex.Message}",
                        result: true);
                }
            }

            return response;
        }


        /// <summary>
        /// Obtiene el período contable abierto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad activa.</param>
        /// <returns>Año y mes del primer período abierto.</returns>
        public ErrorDto<CntxPeriodoActualDto> Cntx_PeriodoActual_Obtener(int codEmpresa,int cod_contabilidad)
        {
            const string sql = @"SELECT TOP 1
              anio,
              mes
          FROM CntX_Periodos
          WHERE cod_contabilidad = @cod_contabilidad
          AND estado = 'P'
          ORDER BY anio ASC, mes ASC";

            return DbHelper.ExecuteSingleQuery<CntxPeriodoActualDto>(
                _portalDb, codEmpresa, sql, null, new { cod_contabilidad });
        }
    }
}
