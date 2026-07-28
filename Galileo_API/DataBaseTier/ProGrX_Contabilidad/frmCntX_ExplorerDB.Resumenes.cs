using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Asientos resumen
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="anio">Año contable de la consulta.</param>
        /// <param name="mes">Mes contable de la consulta.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa, int cod_contabilidad, int anio, int mes)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxAsientoResumenDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = cn.Query<CntxAsientoResumenDto>(
                    "spCntx_Consulta_Asientos_Rsm",
                    new
                    {
                        Contabilidad = cod_contabilidad,
                        Anio = anio,
                        Mes = mes
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxAsientoResumenDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Catalogo Resumen
        /// </summary>
        /// <param name="request">Datos y contexto de la consulta solicitada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxCatalogoResumenDto>> Catalogo_Resumen(CatalogoResumenRequest request)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxCatalogoResumenDto>());

            try
            {
                if (request.codEmpresa == null)
                {
                    response.Code = -1;
                    response.Description = "La empresa es requerida.";
                    return response;
                }

                using var cn = DbHelper.OpenConnection(_portalDb, request.codEmpresa.Value);

                var sql = @"
                        SELECT TOP 50
                            TC.tipo_cuenta        AS codigo,
                            TC.descripcion        AS descripcion,
                            TC.descripcion        AS clasificacion,
                            COUNT(D.num_linea)    AS movimientos,
                            ISNULL(SUM(D.monto_debito),0)  AS total_debitos,
                            ISNULL(SUM(D.monto_credito),0) AS total_creditos,
                            ISNULL(SUM(D.monto_debito - D.monto_credito),0) AS diferencia
                        FROM CntX_Tipos_Cuentas TC
                        LEFT JOIN CntX_Cuentas C
                            ON C.tipo_cuenta = TC.tipo_cuenta
                            AND C.cod_contabilidad = @cod_contabilidad
                        LEFT JOIN CntX_Asientos_Detalle D
                            ON D.cod_cuenta = C.cod_cuenta
                            AND D.cod_contabilidad = @cod_contabilidad
                        LEFT JOIN CntX_Asientos A
                            ON A.cod_contabilidad = D.cod_contabilidad
                            AND A.tipo_asiento = D.tipo_asiento
                            AND A.num_asiento = D.num_asiento
                        WHERE TC.cod_contabilidad = @cod_contabilidad
                          AND (@fechaDesde IS NULL OR A.fecha_asiento >= @fechaDesde)
                          AND (@fechaHasta IS NULL OR A.fecha_asiento <= @fechaHasta)
                        GROUP BY TC.tipo_cuenta, TC.descripcion
                        ORDER BY TC.tipo_cuenta";

                response.Result = cn.Query<CntxCatalogoResumenDto>(
                    sql,
                    new
                    {
                        request.cod_contabilidad,
                        request.fechaDesde,
                        request.fechaHasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxCatalogoResumenDto>>(ex.Message);
            }

            return response;
        }
        /// <summary>
        /// Areas de Trabajo por Padre
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<AreaTrabajoDto>> AreasTrabajo_ObtenerPorPadre(int codEmpresa, int codContabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<AreaTrabajoDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"
            SELECT
                cod_area,
                descripcion,
                CAST(0 AS bit) AS es_padre
            FROM CntX_Area_Definicion
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_area";

                response.Result = cn.Query<AreaTrabajoDto>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<AreaTrabajoDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtiene las cuentas asignadas al área de trabajo seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="codArea">Código del área de trabajo seleccionada.</param>
        /// <returns>Cuentas y estado de aceptación de movimientos del área.</returns>
        public ErrorDto<List<AreaCuentaDto>> AreasTrabajo_Cuentas(int codEmpresa, int codContabilidad, int codArea)
        {
            var response = DbHelper.CreateOkResponse(new List<AreaCuentaDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        C.cod_cuenta_mask AS cuenta,
                        C.descripcion,
                        CASE WHEN C.acepta_movimientos = 1 THEN 'Sí' ELSE 'No' END AS acepta_movimientos
                    FROM CntX_Cuentas C
                    INNER JOIN CntX_Area_Cuentas A
                        ON C.cod_cuenta = A.cod_cuenta
                        AND C.cod_contabilidad = A.cod_contabilidad
                    WHERE A.cod_contabilidad = @codContabilidad
                        AND A.cod_area = @codArea
                    ORDER BY A.cod_cuenta";

                response.Result = cn.Query<AreaCuentaDto>(
                    sql,
                    new { codContabilidad, codArea }
                ).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<AreaCuentaDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Areas de trabajo resumen
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="codArea">Código del área de trabajo seleccionada.</param>
        /// <param name="fechaDesde">Valor de fechaDesde requerido por la consulta.</param>
        /// <param name="fechaHasta">Valor de fechaHasta requerido por la consulta.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<AreaResumenDto>> AreasTrabajo_Resumen(int codEmpresa, int codContabilidad, int codArea, DateTime fechaDesde, DateTime fechaHasta)
        {
            var response = DbHelper.CreateOkResponse(new List<AreaResumenDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"
        EXEC spCntX_Areas_Resumen
            @codContabilidad,
            @codArea,
            @fechaDesde,
            @fechaHasta";

                response.Result = cn.Query<AreaResumenDto>(
                    sql,
                    new
                    {
                        codContabilidad,
                        codArea,
                        fechaDesde,
                        fechaHasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<AreaResumenDto>>(ex.Message);
            }

            return response;
        }
    }
}
