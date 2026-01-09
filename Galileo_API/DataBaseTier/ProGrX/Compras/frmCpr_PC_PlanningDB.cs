using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCprPCPlanningDB
    {
        private readonly PortalDB _portalDB;

        // Sonar: evitar literales repetidos
        private const string ParamOff = "Offset";
        private const string ParamTake = "Fetch";

        public FrmCprPCPlanningDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        // Helper: usa DbHelper.WithConn y devuelve ErrorDto plano (sin ErrorDto<ErrorDto>)
        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? (r.Result ?? DbHelper.ErrorResponse("Error desconocido.", -1))
                : DbHelper.ErrorResponse(r.Description ?? "Error desconocido.", -1);
        }

        // Helper: igual pero para resultados tipados
        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? new ErrorDto<T> { Code = 0, Description = "Ok", Result = r.Result }
                : new ErrorDto<T> { Code = -1, Description = r.Description, Result = default };
        }

        // ===========================
        //  HELPERS (anti-duplication)
        // ===========================

        private static string? NormalizePeriodo(string? periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                return null;

            return string.Equals(periodo, "Todos", StringComparison.OrdinalIgnoreCase)
                ? null
                : periodo;
        }

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return null;

            var f = filtro.Trim();
            return f.Length == 0 ? null : $"%{f}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int? pagina, int? paginacion)
        {
            // Keep current semantics: `pagina` is treated as OFFSET.
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina.Value, paginacion.Value);
        }

        private static void AddPaging(DynamicParameters dp, int? pagina, int? paginacion)
        {
            var (off, take) = NormalizePaging(pagina, paginacion);
            dp.Add(ParamOff, off, DbType.Int32);
            dp.Add(ParamTake, take, DbType.Int32);
        }

        private static (int Total, List<T> Rows) QueryPaged<T>(SqlConnection conn, DynamicParameters dp, string countSql, string dataSql)
        {
            var total = conn.ExecuteScalar<int>(countSql, dp);
            var rows = conn.Query<T>(dataSql, dp).ToList();
            return (total, rows);
        }

        public ErrorDto<List<CprPlanComprasDto>> CprPlanCompras_Obtener(int CodEmpresa)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"SELECT * FROM CPR_PLAN_COMPRAS;";
                    return conn.Query<CprPlanComprasDto>(sql).ToList();
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CprPlanComprasDto>> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto<CprPlanDTDto> CprPlanDT_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto;";
                    var existe = conn.ExecuteScalar<int>(existeSql, new { id_pc = PlanCompras, cod_producto = CodProducto });

                    // Sonar: no lanzar System.Exception
                    if (existe <= 0)
                        throw new InvalidOperationException("Producto sin registrar");

                    const string planSql = @"SELECT * FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto;";
                    var plan = conn.QueryFirstOrDefault<CprPlanDTDto>(planSql, new { id_pc = PlanCompras, cod_producto = CodProducto });

                    if (plan == null)
                        throw new InvalidOperationException("No se pudo obtener la información del producto.");

                    // UEN correspondiente
                    const string uenSql = @"
                        SELECT DISTINCT
                            CASE
                                WHEN ISNULL(PC.COD_UNIDAD_DESTINO,'') = '' THEN PC.COD_UNIDAD
                                ELSE PC.COD_UNIDAD_DESTINO
                            END AS UEN
                        FROM CPR_PLAN_COMPRAS PC
                        WHERE ID_PC = @id_pc;";

                    var uen = conn.QueryFirstOrDefault<string>(uenSql, new { id_pc = PlanCompras }) ?? "";

                    // Totales (tránsito/reservada/entregada)
                    const string totalesSql = @"
                        SELECT T.*,
                               (SELECT COUNT(A.COD_PRODUCTO)
                                  FROM PV_CONTROL_ACTIVOS A
                                 WHERE A.COD_UEN = T.UEN
                                   AND A.COD_PRODUCTO = T.COD_PRODUCTO
                                   AND ISNULL(A.ENTREGA_USUARIO,'') = '') AS QTY_RECERVADA,
                               (SELECT COUNT(A.COD_PRODUCTO)
                                  FROM PV_CONTROL_ACTIVOS A
                                 WHERE A.COD_UEN = T.UEN
                                   AND A.COD_PRODUCTO = T.COD_PRODUCTO
                                   AND ISNULL(A.ENTREGA_USUARIO,'') <> '') AS QTY_ENTREGADA
                        FROM
                        (
                            SELECT DISTINCT
                                   PC.ID_PC,
                                   SP.ADJUDICA_ORDEN,
                                   D.COD_PRODUCTO,
                                   CASE
                                       WHEN ISNULL(PC.COD_UNIDAD_DESTINO,'') = '' THEN PC.COD_UNIDAD
                                       ELSE PC.COD_UNIDAD_DESTINO
                                   END AS UEN,
                                   D.CANTIDAD AS QTY_SOLICITADA,
                                   (SELECT CANTIDAD_TOTAL
                                      FROM CPR_PLAN_DT P
                                     WHERE P.COD_PRODUCTO = @cod_producto
                                       AND P.ID_PC = PC.ID_PC) AS QTY_PLAN_COMPRAS
                              FROM CPR_ORDENES_DETALLE D
                              LEFT JOIN CPR_SOLICITUD_PROV SP ON SP.ADJUDICA_ORDEN = D.COD_ORDEN
                              LEFT JOIN CPR_SOLICITUD_BS SB ON SP.CPR_ID = SB.CPR_ID
                              LEFT JOIN CPR_PLAN_COMPRAS PC ON PC.COD_UNIDAD = SB.COD_UNIDAD
                             WHERE D.COD_PRODUCTO = @cod_producto
                               AND PC.ID_PC IN (SELECT ID_PC FROM CPR_PLAN_DT WHERE COD_PRODUCTO = @cod_producto)
                        ) T
                        WHERE T.UEN = @uen;";

                    var totales = conn.QueryFirstOrDefault<CprPlanDTTotalesData>(totalesSql, new { cod_producto = CodProducto, uen });

                    plan.cantidad_transito = totales?.qty_solicitada ?? 0;
                    plan.cantidad_reservada = totales?.qty_recervada ?? 0;
                    plan.cantidad_despachada = totales?.qty_entregada ?? 0;

                    return plan;
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprPlanDTDto> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto<List<CprPlanDTCortesDto>> CprPlanDTCortes_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string idPlanSql = @"
                        SELECT COALESCE((SELECT ID_PLAN FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto), 0) AS ID_PLAN;";

                    var idPlan = conn.ExecuteScalar<int>(idPlanSql, new { id_pc = PlanCompras, cod_producto = CodProducto });

                    int existe = 0;
                    if (idPlan != 0)
                    {
                        const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
                        existe = conn.ExecuteScalar<int>(existeSql, new { id_plan = idPlan });
                    }

                    if (idPlan != 0 && existe > 0)
                    {
                        const string cortesSql = @"SELECT corte, cantidad, monto FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
                        return conn.Query<CprPlanDTCortesDto>(cortesSql, new { id_plan = idPlan }).ToList();
                    }

                    // Si no hay cortes guardados: genera el rango de cortes según periodo del plan compras
                    const string dateRangeSql = @"
                        WITH DateRange AS
                        (
                            SELECT CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, P.INICIO) + 1, -1)) AS corte
                              FROM CPR_PLAN_PERIODOS P
                              INNER JOIN CPR_PLAN_COMPRAS C ON P.ID_PERIODO = C.ID_PERIODO
                             WHERE C.ID_PC = @id_pc
                            UNION ALL
                            SELECT CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, DATEADD(MONTH, 1, corte)) + 1, -1))
                              FROM DateRange
                             WHERE DATEADD(MONTH, 1, corte) <=
                                   (
                                       SELECT CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, CORTE) + 1, -1))
                                         FROM CPR_PLAN_PERIODOS P
                                         INNER JOIN CPR_PLAN_COMPRAS C ON P.ID_PERIODO = C.ID_PERIODO
                                        WHERE C.ID_PC = @id_pc
                                   )
                        )
                        SELECT corte, 0 AS cantidad, 0 AS monto
                          FROM DateRange
                         ORDER BY corte;";

                    return conn.Query<CprPlanDTCortesDto>(dateRangeSql, new { id_pc = PlanCompras }).ToList();
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CprPlanDTCortesDto>> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto CprPlanCompras_Insert(int CodEmpresa, CprPlanComprasDto request)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"
                        INSERT INTO CPR_PLAN_COMPRAS (ID_PERIODO, COD_UNIDAD, COD_UNIDAD_DESTINO, ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES (@id_periodo, @cod_unidad, @cod_unidad_destino, 'P', GETDATE(), @registro_usuario);";

                    conn.Execute(sql, new
                    {
                        request.id_periodo,
                        request.cod_unidad,
                        request.cod_unidad_destino,
                        request.registro_usuario
                    });

                    return DbHelper.OkResponse("Plan de compras agregado satisfactoriamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto CprPlanCompras_Update(int CodEmpresa, CprPlanComprasDto request)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"
                        UPDATE CPR_PLAN_COMPRAS
                           SET ID_PERIODO = @id_periodo,
                               COD_UNIDAD = @cod_unidad,
                               COD_UNIDAD_DESTINO = @cod_unidad_destino,
                               MODIFICA_FECHA = GETDATE(),
                               MODIFICA_USUARIO = @modifica_usuario
                         WHERE ID_PC = @id_pc;";

                    conn.Execute(sql, new
                    {
                        request.id_periodo,
                        request.cod_unidad,
                        request.cod_unidad_destino,
                        request.modifica_usuario,
                        request.id_pc
                    });

                    return DbHelper.OkResponse("Plan de compras actualizado satisfactoriamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto CprPlanDT_Upsert(int CodEmpresa, string parametros, List<CprPlanDTCortesDto> cortes)
        {
            try
            {
                var plan = JsonConvert.DeserializeObject<CprPlanDTUpsert>(parametros) ?? new CprPlanDTUpsert();

                return WithConn(CodEmpresa, conn =>
                {
                    int cantidadTotal = 0;
                    decimal montoTotal = 0;

                    foreach (var item in cortes ?? new List<CprPlanDTCortesDto>())
                    {
                        cantidadTotal += item.cantidad;
                        montoTotal += item.monto * item.cantidad;
                    }

                    const string spSql = @"exec spCPR_Plan_DT_Upsert @IdPC, @CodProducto, @MontoUnitario, @CantidadTotal, @MontoTotal, @Usuario;";

                    var spParams = new DynamicParameters();
                    spParams.Add("IdPC", plan.id_pc, DbType.Int32);
                    spParams.Add("CodProducto", plan.cod_producto, DbType.String);
                    spParams.Add("MontoUnitario", plan.monto_unitario, DbType.Decimal);
                    spParams.Add("CantidadTotal", cantidadTotal, DbType.Int32);
                    spParams.Add("MontoTotal", montoTotal, DbType.Decimal);
                    spParams.Add("Usuario", plan.usuario, DbType.String);

                    var idPlan = conn.Query<int>(spSql, spParams).FirstOrDefault();

                    const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
                    var existe = conn.ExecuteScalar<int>(existeSql, new { id_plan = idPlan });

                    foreach (var item in cortes ?? new List<CprPlanDTCortesDto>())
                    {
                        if (existe > 0)
                        {
                            const string upd = @"
                                UPDATE CPR_PLAN_DT_CORTES
                                   SET CANTIDAD = @cantidad,
                                       MONTO = @monto,
                                       MODIFICA_FECHA = GETDATE(),
                                       MODIFICA_USUARIO = @usuario
                                 WHERE ID_PLAN = @id_plan AND CORTE = @corte;";

                            conn.Execute(upd, new
                            {
                                cantidad = item.cantidad,
                                monto = item.monto,
                                usuario = plan.usuario,
                                id_plan = idPlan,
                                corte = item.corte
                            });
                        }
                        else
                        {
                            const string ins = @"
                                INSERT INTO CPR_PLAN_DT_CORTES (CORTE, ID_PLAN, CANTIDAD, MONTO, REGISTRO_FECHA, REGISTRO_USUARIO)
                                VALUES (@corte, @id_plan, @cantidad, @monto, GETDATE(), @usuario);";

                            conn.Execute(ins, new
                            {
                                corte = item.corte,
                                id_plan = idPlan,
                                cantidad = item.cantidad,
                                monto = item.monto,
                                usuario = plan.usuario
                            });
                        }
                    }

                    return DbHelper.OkResponse("Plan actualizado satisfactoriamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_Obtener(int CodEmpresa, string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<CprPlanFiltros>(parametros) ?? new CprPlanFiltros();

                return WithConn(CodEmpresa, conn =>
                {
                    var result = new CprResumenPlanLista();

                    var corte = NormalizePeriodo(filtros.periodo);
                    var q = NormalizeLike(filtros.filtro);

                    var dp = new DynamicParameters();
                    dp.Add("IdPc", filtros.planCompras, DbType.Int32);
                    dp.Add("Corte", corte, DbType.String);
                    dp.Add("Q", q, DbType.String);

                    AddPaging(dp, filtros.pagina, filtros.paginacion);

                    const string countSql = @"
                        SELECT COUNT(D.COD_PRODUCTO)
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                         WHERE D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR S.CORTE = @Corte)
                           AND (@Q IS NULL OR (D.COD_PRODUCTO LIKE @Q OR P.DESCRIPCION LIKE @Q));";

                    const string dataSql = @"
                        SELECT D.COD_PRODUCTO, P.DESCRIPCION, S.CANTIDAD, S.MONTO, S.CORTE
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                         WHERE D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR S.CORTE = @Corte)
                           AND (@Q IS NULL OR (D.COD_PRODUCTO LIKE @Q OR P.DESCRIPCION LIKE @Q))
                         ORDER BY S.CORTE DESC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                    var (total, rows) = QueryPaged<CprResumenPlanDto>(conn, dp, countSql, dataSql);
                    result.Total = total;
                    result.Lineas = rows;
                    return result;
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprResumenPlanLista> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto<CprPlanContableLista> CprPlanContable_Obtener(int CodEmpresa, string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<CprPlanFiltros>(parametros) ?? new CprPlanFiltros();

                return WithConn(CodEmpresa, conn =>
                {
                    var result = new CprPlanContableLista();

                    var corte = NormalizePeriodo(filtros.periodo);
                    var q = NormalizeLike(filtros.filtro);

                    var dp = new DynamicParameters();
                    dp.Add("IdPc", filtros.planCompras, DbType.Int32);
                    dp.Add("Corte", corte, DbType.String);
                    dp.Add("Q", q, DbType.String);

                    AddPaging(dp, filtros.pagina, filtros.paginacion);

                    const string countSql = @"
                        SELECT COUNT(*)
                          FROM (
                                SELECT DISTINCT Z.COD_CUENTA_MASK, Z.DESCRIPCION, S.CORTE
                                  FROM CPR_PLAN_DT D
                                  INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                                  INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                                  INNER JOIN CORE_UENS U ON C.COD_UNIDAD = U.COD_UNIDAD
                                  INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                                  INNER JOIN PV_PROD_CLASIFICA B ON P.COD_PRODCLAS = B.COD_PRODCLAS
                                  INNER JOIN CNTX_CUENTAS Z ON B.COD_CUENTA = Z.COD_CUENTA
                                 WHERE D.ID_PC = @IdPc
                                   AND (@Corte IS NULL OR S.CORTE = @Corte)
                                   AND (@Q IS NULL OR (Z.COD_CUENTA_MASK LIKE @Q OR Z.DESCRIPCION LIKE @Q))
                               ) T;";

                    const string dataSql = @"
                        SELECT DISTINCT
                               Z.COD_CUENTA_MASK AS CUENTA,
                               Z.DESCRIPCION,
                               U.CNTX_UNIDAD AS UNIDAD,
                               U.CNTX_CENTRO_COSTO AS CENTRO_COSTO,
                               (S.MONTO * S.CANTIDAD) AS TOTAL,
                               S.CORTE
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN CORE_UENS U ON C.COD_UNIDAD = U.COD_UNIDAD
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                          INNER JOIN PV_PROD_CLASIFICA B ON P.COD_PRODCLAS = B.COD_PRODCLAS
                          INNER JOIN CNTX_CUENTAS Z ON B.COD_CUENTA = Z.COD_CUENTA
                         WHERE D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR S.CORTE = @Corte)
                           AND (@Q IS NULL OR (Z.COD_CUENTA_MASK LIKE @Q OR Z.DESCRIPCION LIKE @Q))
                         ORDER BY S.CORTE DESC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                    var (total, rows) = QueryPaged<CprPlanContableDto>(conn, dp, countSql, dataSql);
                    result.Total = total;
                    result.Lineas = rows;
                    return result;
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprPlanContableLista> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto<CprBitacoraLista> CprBitacora_Obtener(int CodEmpresa, string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<CprPlanFiltros>(parametros) ?? new CprPlanFiltros();

                return WithConn(CodEmpresa, conn =>
                {
                    var result = new CprBitacoraLista();

                    var mov = $"%Plan:{filtros.planCompras}%";
                    var q = NormalizeLike(filtros.filtro);

                    var dp = new DynamicParameters();
                    dp.Add("Mov", mov, DbType.String);
                    dp.Add("Q", q, DbType.String);

                    AddPaging(dp, filtros.pagina, filtros.paginacion);

                    const string countSql = @"
                        SELECT COUNT(*)
                          FROM CPR_BITACORA_SOLICITUD
                         WHERE MOVIMIENTO LIKE @Mov
                           AND (@Q IS NULL OR (USUARIO LIKE @Q OR DETALLE LIKE @Q OR CONVERT(VARCHAR(30), FECHAHORA, 120) LIKE @Q));";

                    const string dataSql = @"
                        SELECT ID_BITACORA, FECHAHORA, USUARIO, DETALLE
                          FROM CPR_BITACORA_SOLICITUD
                         WHERE MOVIMIENTO LIKE @Mov
                           AND (@Q IS NULL OR (USUARIO LIKE @Q OR DETALLE LIKE @Q OR CONVERT(VARCHAR(30), FECHAHORA, 120) LIKE @Q))
                         ORDER BY FECHAHORA DESC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                    var (total, rows) = QueryPaged<CprBitacoraDto>(conn, dp, countSql, dataSql);
                    result.Total = total;
                    result.Lineas = rows;
                    return result;
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprBitacoraLista> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_ObtenerxCuenta(int CodEmpresa, string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<CprPlanFiltros>(parametros) ?? new CprPlanFiltros();

                return WithConn(CodEmpresa, conn =>
                {
                    var result = new CprResumenPlanLista();
                    var dp = new DynamicParameters();

                    // Trae COD_PRODCLAS para la cuenta
                    const string prodClasSql = @"SELECT TOP 1 COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE COD_CUENTA = @cod_cuenta;";
                    var prodclas = conn.QueryFirstOrDefault<int?>(prodClasSql, new { cod_cuenta = filtros.filtro }) ?? 0;

                    if (prodclas <= 0)
                    {
                        result.Total = 0;
                        result.Lineas = new List<CprResumenPlanDto>();
                        return result;
                    }

                    var corte = NormalizePeriodo(filtros.periodo);

                    dp.Add("ProdClas", prodclas, DbType.Int32);
                    dp.Add("IdPc", filtros.planCompras, DbType.Int32);
                    dp.Add("Corte", corte, DbType.String);

                    AddPaging(dp, filtros.pagina, filtros.paginacion);

                    const string countSql = @"
                        SELECT COUNT(D.COD_PRODUCTO)
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                         WHERE P.COD_PRODCLAS = @ProdClas
                           AND D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR S.CORTE = @Corte);";

                    const string dataSql = @"
                        SELECT D.COD_PRODUCTO, P.DESCRIPCION, S.CANTIDAD, S.MONTO, S.CORTE
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                         WHERE P.COD_PRODCLAS = @ProdClas
                           AND D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR S.CORTE = @Corte)
                         ORDER BY S.CORTE DESC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                    var (total, rows) = QueryPaged<CprResumenPlanDto>(conn, dp, countSql, dataSql);
                    result.Total = total;
                    result.Lineas = rows;
                    return result;
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprResumenPlanLista> { Code = -1, Description = ex.Message, Result = null };
            }
        }
    }
}