using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Arrendamientos
{
    public class FrmArfTrasladoAsientosDb
    {
        private readonly PortalDB _portalDB;

        public FrmArfTrasladoAsientosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Busca los asientos de ARF que todavía no han sido trasladados a contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="filtros">Rango de fechas o indicador para consultar todas las fechas.</param>
        /// <returns>Lista de asientos pendientes de traslado.</returns>
        public ErrorDto<List<ArfTrasladoTablaDto>> Buscar(
            int codEmpresa,
            ArfTrasladoFiltroDto filtros)
        {
            var sql = new StringBuilder(@"
                    SELECT
                        COD_CONTABILIDAD AS cod_contabilidad,
                        tipo_asiento,
                        num_asiento,
                        fecha,
                        referencia,
                        notas
                    FROM ARF_ASIENTOS
                    WHERE Traslado_Fecha IS NULL
                ");

            if (filtros.todos == false &&
                filtros.fechaInicio.HasValue &&
                filtros.fechaCorte.HasValue)
            {
                sql.Append(@"
                        AND fecha BETWEEN @fechaInicio AND @fechaCorte
                    ");
            }

            sql.Append(" ORDER BY fecha, num_asiento ");

            return DbHelper.ExecuteListQuery<ArfTrasladoTablaDto>(
                _portalDB,
                codEmpresa,
                sql.ToString(),
                new
                {
                    fechaInicio = filtros.fechaInicio?.Date,
                    fechaCorte = filtros.fechaCorte?.Date.AddDays(1).AddSeconds(-1)
                });
        }

        /// <summary>
        /// Traslada los asientos seleccionados de ARF a contabilidad y omite los de período cerrado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="asientos">Asientos seleccionados y usuario responsable del traslado.</param>
        /// <returns>Resultado del traslado, incluyendo advertencia si hubo períodos cerrados.</returns>
        public ErrorDto<bool> Trasladar(
            int codEmpresa,
            List<ArfTrasladoRequestDto> asientos)
        {
            var proceso = DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                cn =>
                {
                    var trasladados = 0;
                    var periodosCerrados = 0;

                    foreach (var item in asientos)
                    {
                        var fecha = cn.QueryFirstOrDefault<DateTime?>(
                            @"SELECT fecha
                              FROM ARF_ASIENTOS
                              WHERE Traslado_Fecha IS NULL
                                AND COD_CONTABILIDAD = @cod_contabilidad
                                AND tipo_asiento = @tipo_asiento
                                AND num_asiento = @num_asiento",
                            item);

                        if (!fecha.HasValue)
                        {
                            continue;
                        }

                        var periodoAbierto = cn.QueryFirstOrDefault<int?>(
                            @"SELECT TOP 1 1
                              FROM CntX_Periodos
                              WHERE anio = @anio
                                AND mes = @mes
                                AND estado = 'P'
                                AND cod_contabilidad = @cod_contabilidad",
                            new
                            {
                                anio = fecha.Value.Year,
                                mes = fecha.Value.Month,
                                item.cod_contabilidad
                            });

                        if (!periodoAbierto.HasValue)
                        {
                            periodosCerrados++;
                            continue;
                        }

                        using var tx = cn.BeginTransaction();

                        try
                        {
                            const string sql = @"
                                INSERT INTO CntX_Asientos
                                    (COD_CONTABILIDAD, tipo_asiento, num_asiento, anio, mes,
                                     fecha_asiento, descripcion, balanceado, notas, modulo,
                                     user_crea, referencia)
                                SELECT COD_CONTABILIDAD, tipo_asiento, num_asiento,
                                       YEAR(fecha), MONTH(fecha), fecha,
                                       SUBSTRING(num_asiento + '...' + referencia, 1, 100),
                                       'S', notas, 20, registro_usuario, referencia
                                FROM ARF_ASIENTOS
                                WHERE COD_CONTABILIDAD = @cod_contabilidad
                                  AND tipo_asiento = @tipo_asiento
                                  AND num_asiento = @num_asiento;

                                INSERT INTO CntX_Asientos_detalle
                                    (num_linea, COD_CONTABILIDAD, tipo_Asiento, num_asiento,
                                     cod_cuenta, documento, detalle, tipo_Cambio, monto_Debito,
                                     monto_credito, cod_unidad, cod_divisa, cod_centro_costo)
                                SELECT Linea_Id, COD_CONTABILIDAD, tipo_asiento, num_asiento,
                                       cod_cuenta, SUBSTRING(documento, 1, 35),
                                       SUBSTRING(detalle, 1, 100), ISNULL(tipo_Cambio, 1),
                                       CASE WHEN Movimiento = 'D' THEN Monto ELSE 0 END,
                                       CASE WHEN Movimiento = 'C' THEN Monto ELSE 0 END,
                                       cod_unidad, cod_divisa, cod_centro_costo
                                FROM ARF_ASIENTOS_DETALLE
                                WHERE COD_CONTABILIDAD = @cod_contabilidad
                                  AND tipo_asiento = @tipo_asiento
                                  AND num_asiento = @num_asiento;

                                UPDATE ARF_ASIENTOS
                                SET Traslado_Fecha = GETDATE(),
                                    Traslado_Usuario = @usuario
                                WHERE COD_CONTABILIDAD = @cod_contabilidad
                                  AND tipo_asiento = @tipo_asiento
                                  AND num_asiento = @num_asiento;";

                            cn.Execute(sql, item, tx);
                            tx.Commit();
                            trasladados++;
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }

                    return new
                    {
                        Trasladados = trasladados,
                        PeriodosCerrados = periodosCerrados
                    };
                });

            if (proceso.Code == -1 || proceso.Result == null)
            {
                return DbHelper.CreateErrorResponse<bool>(proceso.Description);
            }

            var descripcion = proceso.Result.PeriodosCerrados switch
            {
                > 0 when proceso.Result.Trasladados > 0 =>
                    "Algunos asientos no se trasladaron porque el período contable está cerrado.",
                > 0 =>
                    "No se trasladaron asientos porque el período contable está cerrado.",
                _ => "Ok"
            };

            return DbHelper.CreateOkResponse(proceso.Result.Trasladados > 0, descripcion);
        }
    }
}
