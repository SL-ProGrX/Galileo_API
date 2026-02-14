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
        /// Busca los asientos disponibles para traslardar asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>

        public ErrorDto<List<ArfTrasladoTablaDto>> Buscar(int codEmpresa,ArfTrasladoFiltroDto filtros)
        {
            var response = new ErrorDto<List<ArfTrasladoTablaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

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

                if (!filtros.todos &&
                    filtros.fechaInicio.HasValue &&
                    filtros.fechaCorte.HasValue)
                {
                    sql.Append(@"
                        AND fecha BETWEEN @fechaInicio AND @fechaCorte
                    ");
                }

                sql.Append(" ORDER BY fecha, num_asiento ");

                response.Result = cn.Query<ArfTrasladoTablaDto>(
                    sql.ToString(),
                    new
                    {
                        fechaInicio = filtros.fechaInicio?.Date,
                        fechaCorte = filtros.fechaCorte?.Date.AddDays(1).AddSeconds(-1)
                    }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Traslado de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="asientos"></param>
        /// <returns></returns>

        public ErrorDto<bool> Trasladar(int codEmpresa,List<ArfTrasladoRequestDto> asientos)
        {
            var response = new ErrorDto<bool>();

            using var cn = new SqlConnection(
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
            );

            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                foreach (var item in asientos)
                {
                    var sql = @"

                            INSERT INTO CntX_Asientos
                            SELECT COD_CONTABILIDAD, tipo_asiento, num_asiento,
                                   YEAR(fecha), MONTH(fecha), fecha,
                                   SUBSTRING(num_asiento + '...' + referencia,1,100),
                                   'S', notas, 20, registro_usuario, referencia
                            FROM ARF_ASIENTOS
                            WHERE COD_CONTABILIDAD = @cod_contabilidad
                              AND tipo_asiento = @tipo_asiento
                              AND num_asiento = @num_asiento;

                            INSERT INTO CntX_Asientos_detalle
                            SELECT Linea_Id, COD_CONTABILIDAD, tipo_asiento,
                                   num_asiento, cod_cuenta,
                                   SUBSTRING(documento,1,35),
                                   SUBSTRING(detalle,1,100),
                                   ISNULL(tipo_Cambio,1),
                                   CASE WHEN Movimiento='D' THEN Monto ELSE 0 END,
                                   CASE WHEN Movimiento='C' THEN Monto ELSE 0 END,
                                   cod_unidad, cod_divisa, cod_centro_costo
                            FROM ARF_ASIENTOS_DETALLE
                            WHERE COD_CONTABILIDAD = @cod_contabilidad
                              AND tipo_asiento = @tipo_asiento
                              AND num_asiento = @num_asiento;

                            UPDATE ARF_ASIENTOS
                            SET Traslado_Fecha = GETDATE()
                            WHERE COD_CONTABILIDAD = @cod_contabilidad
                              AND tipo_asiento = @tipo_asiento
                              AND num_asiento = @num_asiento;
                            ";

                    cn.Execute(sql, item, tx);
                }

                tx.Commit();
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
