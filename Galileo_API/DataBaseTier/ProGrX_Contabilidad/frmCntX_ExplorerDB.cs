using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXExploradorContableDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXExploradorContableDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXExploradorContableDb(
            PortalDB portalDb,
            MSecurityMainDb securityDb)
        {
            _portalDb = portalDb;
        }

      

        /// <summary>
        /// Obtiene cuentas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Cuentas_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT cod_cuenta AS Item,
                                   descripcion AS Descripcion
                            FROM CntX_Cuentas
                            WHERE cod_contabilidad = @codEmpresa
                            ORDER BY cod_cuenta";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql,
                    new { codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene tipo de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxTipoAsientoDto>> Cntx_TiposAsiento_Obtener(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<CntxTipoAsientoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT
                        tipo_asiento,
                        descripcion,
                        consecutivo
                            FROM CntX_Tipos_Asientos
                            WHERE cod_contabilidad = @cod_contabilidad
                            ORDER BY tipo_asiento";

                response.Result = cn.Query<CntxTipoAsientoDto>(sql,
                    new { cod_contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPeriodoDto>> Cntx_Periodos_Obtener(int codEmpresa, int cod_contabilidad, string estado)
        {
            var response = new ErrorDto<List<CntxPeriodoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT 
                                ANIO AS anio,
                                MES AS mes,
                                PERIODO_CORTE AS fecha_corte,

                                CASE 
                                    WHEN ESTADO = 'C' THEN 'CERRADO'
                                    ELSE 'ABIERTO'
                                END AS cerrado

                            FROM CNTX_PERIODOS
                            WHERE COD_CONTABILIDAD = @cod_contabilidad
                            ORDER BY ANIO, MES";

                response.Result = cn.Query<CntxPeriodoDto>(sql,
                    new { cod_contabilidad, estado }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

  
        /// <summary>
        /// Asientos listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoRsmDto>> Cntx_Asientos_Listar(int codEmpresa, int cod_contabilidad, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoRsmDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sb = new StringBuilder();

                sb.Append(@"
                    SELECT 
                        A.num_asiento,
                        A.tipo_asiento,
                        A.fecha_asiento,
                        A.descripcion,
                        ISNULL(SUM(D.monto_debito),0) AS Debe,
                        ISNULL(SUM(D.monto_credito),0) AS Haber,
                        CASE WHEN A.fecha_aplicado IS NULL THEN 'NO' ELSE 'SI' END AS Aplicado
                    FROM CntX_Asientos A
                    LEFT JOIN CntX_Asientos_Detalle D 
                        ON A.cod_contabilidad = D.cod_contabilidad
                        AND A.tipo_asiento = D.tipo_asiento
                        AND A.num_asiento = D.num_asiento
                    WHERE A.cod_contabilidad = @cod_contabilidad
                ");

                // ==== FILTROS DINÁMICOS ====

                if (!string.IsNullOrWhiteSpace(filtros.cod_tipo_asiento))
                    sb.Append(" AND A.tipo_asiento = @tipo ");

                if (!string.IsNullOrWhiteSpace(filtros.num_asiento))
                    sb.Append(" AND A.num_asiento LIKE '%' + @num + '%' ");

                if (filtros.todas != true && filtros.fecha_desde != null && filtros.fecha_hasta != null)
                {
                    sb.Append(" AND A.fecha_asiento BETWEEN @desde AND @hasta ");
                }

                sb.Append(@"
                    GROUP BY A.num_asiento, A.tipo_asiento, A.fecha_asiento, 
                             A.descripcion, A.fecha_aplicado
                    ORDER BY A.fecha_asiento DESC
                ");

                response.Result = cn.Query<CntxAsientoRsmDto>(
                    sb.ToString(),
                    new
                    {
                        codEmpresa,
                        cod_contabilidad,
                        tipo = filtros.cod_tipo_asiento,
                        num = filtros.num_asiento,
                        desde = filtros.fecha_desde,
                        hasta = filtros.fecha_hasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Asientos detalle Listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(int codEmpresa, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoDetDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                D.COD_CUENTA            AS cod_cuenta,
                C.DESCRIPCION           AS cuenta_descripcion,
                D.MONTO_DEBITO          AS monto_debito,
                D.MONTO_CREDITO         AS monto_credito,
                D.DOCUMENTO             AS documento,
                D.DETALLE               AS detalle,
                D.COD_UNIDAD            AS cod_unidad,
                D.COD_CENTRO_COSTO      AS cod_centro_costo,
                D.COD_DIVISA            AS cod_divisa
            FROM CntX_Asientos_Detalle D
            INNER JOIN CntX_Cuentas C
                ON D.COD_CONTABILIDAD = C.COD_CONTABILIDAD
                AND D.COD_CUENTA = C.COD_CUENTA
            WHERE D.COD_CONTABILIDAD = @codContabilidad
              AND D.TIPO_ASIENTO = @tipo
              AND D.NUM_ASIENTO = @numero
            ORDER BY D.NUM_LINEA";

                response.Result = cn.Query<CntxAsientoDetDto>(sql, new
                {
                    codContabilidad = filtros.cod_contabilidad,
                    tipo = filtros.cod_tipo_asiento,
                    numero = filtros.num_asiento
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la fecha servidor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>

        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = "SELECT dbo.MyGetdate()";

                var fechaServidor = cn.QuerySingle<DateTime>(sql);

                response.Description =
                    fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Cuentas por Padre
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="codCuentaPadre"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(int codEmpresa, int cod_contabilidad, string? codCuentaPadre)
        {
            var response = new ErrorDto<List<CntxCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT  
                cod_cuenta,
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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }




        /// <summary>
        /// Obtiene Asientros TreePorTipo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="tipo"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoTreeDto>> Cntx_Asientos_TreePorTipo(int codEmpresa, int cod_contabilidad, string tipo)
        {
            var response = new ErrorDto<List<CntxAsientoTreeDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var periodo = cn.QueryFirstOrDefault<(int anio, int mes)>(
                @"SELECT TOP 1 anio, mes
                      FROM CntX_Periodos
                      WHERE cod_contabilidad = @cod_contabilidad
                      AND estado = 'P'
                      ORDER BY anio ASC, mes ASC",
                new { cod_contabilidad });

                if (periodo.anio == 0)
                {
                    response.Result = new List<CntxAsientoTreeDto>();
                    return response;
                }

                var sql = @"
                            SELECT 
                                num_asiento,
                                fecha_asiento
                            FROM CntX_Asientos
                            WHERE cod_contabilidad = @cod_contabilidad
                              AND tipo_asiento = @tipo
                              AND anio = @anio
                              AND mes = @mes
                            ORDER BY num_asiento";

                response.Result = cn.Query<CntxAsientoTreeDto>(sql, new
                {
                    cod_contabilidad,
                    tipo,
                    anio = periodo.anio,
                    mes = periodo.mes
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene tipos de Cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxTipoCuentaDto>> Cntx_TiposCuenta_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<CntxTipoCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene cuenta raiz por Tipo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="tipoCuenta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCuentaDto>> Cntx_CuentasRaizPorTipo_Obtener(int codEmpresa, int codContabilidad, string tipoCuenta)
        {
            var response = new ErrorDto<List<CntxCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT  
                            cod_cuenta,
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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Diferidos Obtener
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Diferidos_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT cod_diferido AS Item,
                   descripcion AS Descripcion
            FROM CntX_Diferidos
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_diferido";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }
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
        /// Obtiene plantillas diferidos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codDiferido"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxDiferidoPlantillaDto>> DiferidoPlantillas_Obtener(int codEmpresa,int codContabilidad,int codDiferido)
        {
            var response = new ErrorDto<List<CntxDiferidoPlantillaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
        SELECT 
            COD_DIFPLANTILLA      AS Item,
            DESCRIPCION           AS Descripcion,
            MONTO_DIFERIR         AS Monto,
            ACUMULADO             AS Acumulado,
            (MONTO_DIFERIR - ACUMULADO) AS Pendiente,
            PLAZO                 AS Plazo,
            FECHA_CREA            AS Inicio,
            USER_CREA             AS Usuario,
            DOCUMENTO             AS Documento
        FROM CntX_diferido_plantilla
        WHERE COD_CONTABILIDAD = @codContabilidad
          AND COD_DIFERIDO = @codDiferido
        ORDER BY COD_DIFPLANTILLA";

                response.Result = cn.Query<CntxDiferidoPlantillaDto>(
                    sql,
                    new { codContabilidad, codDiferido }
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
        /// Obtiene diferidos historicos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codDiferido"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxDiferidoHistoricoDto>> DiferidoHistorico_Obtener(int codEmpresa, int codContabilidad, int codDiferido, int codPlantilla)
        {
            var response = new ErrorDto<List<CntxDiferidoHistoricoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT num_asiento,
                   tipo_asiento,
                   fecha,
                   anio,
                   mes,
                   usuario
            FROM CntX_Diferido_Historico
            WHERE cod_contabilidad = @codContabilidad
              AND cod_difPlantilla = @codPlantilla
            ORDER BY anio, mes";

                response.Result = cn.Query<CntxDiferidoHistoricoDto>(
                    sql,
                    new { codContabilidad, codDiferido, codPlantilla }
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
        /// Tipos asientos buscar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }



        /// <summary>
        /// Buscar unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Unidades_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Centro de costo buscar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

      
        /// <summary>
        /// Busca divisas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxDivisaDto>> Cntx_Divisas_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<CntxDivisaDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    
        /// <summary>
        /// Asientos resumen
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa, int cod_contabilidad, int anio, int mes)
        {
            var response = new ErrorDto<List<CntxAsientoResumenDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa)
                );

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Catalogo Resumen
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCatalogoResumenDto>> Catalogo_Resumen(CatalogoResumenRequest request)
        {
            var response = new ErrorDto<List<CntxCatalogoResumenDto>>();

            try
            {
                if (request.codEmpresa == null)
                {
                    response.Code = -1;
                    response.Description = "La empresa es requerida.";
                    return response;
                }

                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(request.codEmpresa.Value)
                );

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Plantilla Rate Obtener
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PlantillaRate_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                cod_plantilla AS Item,
                descripcion   AS Descripcion
            FROM CntX_Plantilla_Rate
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_plantilla";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }
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
        /// Plantilla Rate Detalle
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPlantillaRateDetalleDto>> PlantillaRate_Detalle(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            var response = new ErrorDto<List<CntxPlantillaRateDetalleDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                P.cod_cuenta,
                C.cod_Cuenta_Mask      AS cod_cuenta_mask,
                C.descripcion          AS descripcion,
                P.debitos,
                P.creditos,
                P.detalle
            FROM CntX_Plantilla_Rate_Detalle P
            INNER JOIN CntX_Cuentas C
                ON P.cod_contabilidad = C.cod_contabilidad
                AND P.cod_cuenta = C.cod_cuenta
            WHERE P.cod_contabilidad = @codContabilidad
              AND P.cod_plantilla = @codPlantilla
            ORDER BY P.num_linea";

                response.Result = cn.Query<CntxPlantillaRateDetalleDto>(
                    sql,
                    new { codContabilidad, codPlantilla }
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
        /// Areas de Trabajo por Padre
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAreaPadre"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AreasTrabajo_ObtenerPorPadre(int codEmpresa, int? codAreaPadre)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                cod_area AS Item,
                cod_contabilidad AS Descripcion
            FROM CNTX_AREAS_UNIDADES
            ORDER BY cod_area";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codAreaPadre }
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
        /// Areas de trabajo resumen
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codArea"></param>
        /// <param name="fechaDesde"></param>
        /// <param name="fechaHasta"></param>
        /// <returns></returns>
        public ErrorDto<List<AreaResumenDto>> AreasTrabajo_Resumen(int codEmpresa, int codContabilidad, int codArea, DateTime fechaDesde, DateTime fechaHasta)
        {
            var response = new ErrorDto<List<AreaResumenDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

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
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            var response = new ErrorDto<List<CntxContabilidadDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                        SELECT 
                            cod_contabilidad AS codigo,
                            nombre,
                            tel_central,
                            tel_fax,
                            contacto
                        FROM CntX_Contabilidades
                        ORDER BY cod_contabilidad";

                response.Result = cn.Query<CntxContabilidadDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener Cierres
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCierreDto>> ObtenerCierres(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<CntxCierreDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT
                        INICIO_ANIO      AS in_anio,
                        INICIO_MES       AS in_mes,
                        CORTE_ANIO       AS co_anio,
                        CORTE_MES        AS co_mes,
                        DESCRIPCION      AS descripcion,
                        CUENTA_GANPER    AS gan_per,
                        CUENTA_UTILIDAD  AS exc_uti,
                        CUENTA_IMPRENTA  AS renta_cta,
                        IMPUESTO_RENTA   AS renta,
                        CASE 
                            WHEN ACTIVO = 1 THEN 'Si'
                            ELSE 'No'
                        END AS vigente
                    FROM CNTX_CIERRES
                    WHERE COD_CONTABILIDAD = @cod_contabilidad
                    ORDER BY INICIO_ANIO DESC, INICIO_MES DESC";

                response.Result = cn.Query<CntxCierreDto>(
                    sql,
                    new { cod_contabilidad }
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
        /// Aplica asientos mayorizar
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> Asientos_Mayorizar(CntxMayorizarRequest dto)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(dto.cod_empresa)
                );

                cn.Execute(
                    "sbCntX_Asiento_Mayorizar",
                    new
                    {
                        Tipo_Asiento = dto.tipo_asiento,
                        Num_Asiento = dto.num_asiento,
                        Fecha = dto.fecha_asiento
                    },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene las notas de Asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="tipo_asiento"></param>
        /// <param name="num_asiento"></param>
        /// <returns></returns>
        public ErrorDto<string?> NotasAsiento(int codEmpresa,int cod_contabilidad,string tipo_asiento,int num_asiento)
        {
            var response = new ErrorDto<string?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var result = cn.QueryFirstOrDefault<string>(
                    @"SELECT nota
              FROM Cntx_Asientos
              WHERE cod_contabilidad = @cod_contabilidad
              AND tipo_asiento = @tipo_asiento
              AND num_asiento = @num_asiento",
                    new
                    {
                        cod_contabilidad,
                        tipo_asiento,
                        num_asiento
                    });

                response.Result = result;
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