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
        /// Cuentas por Padre
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="codCuentaPadre">Código de la cuenta madre; nulo para consultar la raíz.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(int codEmpresa, int cod_contabilidad, string? codCuentaPadre)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxCuentaDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"
            SELECT  
                cod_cuenta,
                cod_cuenta_mask,
                descripcion,
                CASE 
                    WHEN acepta_movimientos = 0 THEN 1
                    ELSE 0
                                END AS es_mayor,
                    CASE tipo_cuenta
                                WHEN '01' THEN 'ACTIVOS'
                                WHEN '02' THEN 'PASIVOS'
                                WHEN '03' THEN 'PATRIMONIO'
                                WHEN '04' THEN 'INGRESOS'
                                WHEN '05' THEN 'GASTOS'
                                WHEN '06' THEN 'COSTO VENTAS'
                                WHEN '08' THEN 'CUENTA ORDEN'
                                WHEN '09' THEN 'CUENTA ORDEN'
                                ELSE 'NO DEFINIDO'
                            END AS tipo_descripcion,
                                CASE 
                    WHEN acepta_movimientos = 0 THEN 'NO'
                    WHEN acepta_movimientos = 1 THEN 'SI'
                    ELSE 'NO DEFINIDO'
                END AS acepta_movimientos_desc
            FROM CntX_Cuentas
            WHERE cod_contabilidad = @cod_contabilidad
              AND cuenta_madre = @codCuentaPadre
            ORDER BY cod_cuenta";

                response.Result = cn.Query<CntxCuentaDto>(
                    sql,
                    new { codEmpresa, codCuentaPadre, cod_contabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxCuentaDto>>(ex.Message);
            }

            return response;
        }




        /// <summary>
        /// Obtiene Asientros TreePorTipo
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="tipo">Tipo de asiento seleccionado.</param>
        /// <param name="anio">Año contable de la consulta.</param>
        /// <param name="mes">Mes contable de la consulta.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxAsientoTreeDto>> Cntx_Asientos_TreePorTipo(int codEmpresa, int cod_contabilidad, string tipo, int anio, int mes)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxAsientoTreeDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"
                            SELECT
                                A.num_asiento,
                                A.fecha_asiento,
                                A.descripcion,
                                A.anio,
                                A.mes,
                                A.ts,
                                ISNULL(SUM(D.monto_debito), 0) AS debe,
                                ISNULL(SUM(D.monto_credito), 0) AS haber,
                                CASE WHEN A.fecha_aplicado IS NULL THEN 'NO' ELSE 'SI' END AS aplicado
                            FROM CntX_Asientos A
                            LEFT JOIN CntX_Asientos_Detalle D
                              ON A.cod_contabilidad = D.cod_contabilidad
                             AND A.tipo_asiento = D.tipo_asiento
                             AND A.num_asiento = D.num_asiento
                            WHERE A.cod_contabilidad = @cod_contabilidad
                              AND A.tipo_asiento = @tipo
                              AND A.anio = @anio
                              AND A.mes = @mes
                            GROUP BY A.num_asiento, A.fecha_asiento, A.descripcion,
                                     A.anio, A.mes, A.ts, A.fecha_aplicado
                            ORDER BY A.num_asiento";

                response.Result = cn.Query<CntxAsientoTreeDto>(sql, new
                {
                    cod_contabilidad,
                    tipo,
                    anio,
                    mes
                }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxAsientoTreeDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtiene tipos de Cuenta
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxTipoCuentaDto>> Cntx_TiposCuenta_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxTipoCuentaDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"
            SELECT tipo_cuenta     AS item,
                   descripcion
            FROM CntX_Tipos_Cuentas
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY prioridad";

                response.Result = cn.Query<CntxTipoCuentaDto>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxTipoCuentaDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtiene cuenta raiz por Tipo
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="tipoCuenta">Tipo de cuenta seleccionado.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxCuentaDto>> Cntx_CuentasRaizPorTipo_Obtener(int codEmpresa, int codContabilidad, string tipoCuenta)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxCuentaDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = @"SELECT  
                            cod_cuenta,
                            cod_cuenta_mask,
                            descripcion,

                            CASE 
                                WHEN acepta_movimientos = 0 THEN 1
                                ELSE 0
                            END AS es_mayor,

                            CASE tipo_cuenta
                                WHEN '01' THEN 'ACTIVOS'
                                WHEN '02' THEN 'PASIVOS'
                                WHEN '03' THEN 'PATRIMONIO'
                                WHEN '04' THEN 'INGRESOS'
                                WHEN '05' THEN 'GASTOS'
                                WHEN '06' THEN 'COSTO VENTAS'
                                WHEN '08' THEN 'CUENTA ORDEN'
                                WHEN '09' THEN 'CUENTA ORDEN'
                                ELSE 'NO DEFINIDO'
                            END AS tipo_descripcion,
                                CASE 
                    WHEN acepta_movimientos = 0 THEN 'NO'
                    WHEN acepta_movimientos = 1 THEN 'SI'
                    ELSE 'NO DEFINIDO'
                END AS acepta_movimientos_desc
                        FROM CntX_Cuentas
                        WHERE cod_contabilidad = @codContabilidad
                          AND tipo_cuenta = @tipoCuenta
                            AND ISNULL(cuenta_madre,'') = ''
                        ORDER BY cod_cuenta;";

                response.Result = cn.Query<CntxCuentaDto>(
                    sql,
                    new { codContabilidad, tipoCuenta }
                ).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxCuentaDto>>(ex.Message);
            }

            return response;
        }
        /// <summary>
        /// Tipos asientos buscar
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT tipo_asiento AS item,
                             descripcion   AS descripcion
                      FROM CntX_Tipos_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY tipo_asiento",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

            return response;
        }



        /// <summary>
        /// Buscar unidades
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Unidades_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_unidad AS item,
                             descripcion AS descripcion
                      FROM CntX_Unidades
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_unidad",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

            return response;
        }


        /// <summary>
        /// Centro de costo buscar
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_centro_costo AS item,
                             descripcion       AS descripcion
                      FROM CntX_Centro_Costos
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_centro_costo",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

            return response;
        }

      
        /// <summary>
        /// Busca divisas
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxDivisaDto>> Cntx_Divisas_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxDivisaDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = cn.Query<CntxDivisaDto>(
                    @"SELECT 
                        cod_divisa,
                        descripcion,
                        tc_venta,
                        tc_compra,
                        divisa_local
                    FROM CntX_Divisas
                    WHERE cod_contabilidad = @cod_contabilidad
                    ORDER BY cod_divisa",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxDivisaDto>>(ex.Message);
            }

            return response;
        }
    }
}
