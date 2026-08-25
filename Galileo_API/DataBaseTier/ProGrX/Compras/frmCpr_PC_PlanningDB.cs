using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Galileo.DataBaseTier
{
    public class FrmCprPCPlanningDB
    {
        private const string DefaultErrorDescription = "Error";
        private readonly PortalDB _portalDB;

        public FrmCprPCPlanningDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        private sealed class ResumenPlanRow
        {
            // Populated by Dapper mapping
            public string COD_PRODUCTO = string.Empty;
            public string DESCRIPCION = string.Empty;
            public int CANTIDAD = 0;
            public decimal MONTO = 0m;
            public DateTime CORTE = default;
            public int TotalRows = 0;
        }

        private sealed class PlanContableRow
        {
            // Populated by Dapper mapping
            public string CUENTA = string.Empty;
            public string DESCRIPCION = string.Empty;
            public string UNIDAD = string.Empty;
            public string CENTRO_COSTO = string.Empty;
            public decimal TOTAL = 0m;
            public DateTime CORTE = default;
            public int TotalRows = 0;
        }

        private sealed class BitacoraRow
        {
            // Populated by Dapper mapping
            public int ID_BITACORA = 0;
            public DateTime FECHAHORA = default;
            public string USUARIO = string.Empty;
            public string DETALLE = string.Empty;
            public int TotalRows = 0;
        }

        private sealed class CorteUpsertContext
        {
            public CorteUpsertContext(int codEmpresa, int idPlan, string usuario, bool actualizar)
            {
                CodEmpresa = codEmpresa;
                IdPlan = idPlan;
                Usuario = usuario;
                Actualizar = actualizar;
            }

            public int CodEmpresa { get; }
            public int IdPlan { get; }
            public string Usuario { get; }
            public bool Actualizar { get; }
        }

        private static int CodeOrMinus1<T>(ErrorDto<T> r) => r.Code is int c ? c : -1;
        
        private sealed record PagedArgs(string? Corte, string? Q, int Offset, int Fetch);

        private static CprPlanFiltros ParsePlanFiltros(string parametros)
        {
            try
            {
                return JsonConvert.DeserializeObject<CprPlanFiltros>(parametros) ?? new CprPlanFiltros();
            }
            catch
            {
                return new CprPlanFiltros();
            }
        }

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

        private static PagedArgs BuildPagedArgs(CprPlanFiltros filtros, bool includeCorte)
        {
            var corte = includeCorte ? NormalizePeriodo(filtros.periodo) : null;
            var q = NormalizeLike(filtros.filtro);

            var offset = filtros.pagina.GetValueOrDefault(0);
            if (offset < 0) offset = 0;

            var fetch = filtros.paginacion.GetValueOrDefault(30);
            if (fetch <= 0) fetch = 30;

            return new PagedArgs(corte, q, offset, fetch);
        }

        private static int Cpr_PC_Planning_ResumenSortColumn_Resolver(string? sortField)
        {
            return sortField?.Trim().ToLowerInvariant() switch
            {
                "cod_producto" => 1,
                "descripcion" => 2,
                "cantidad" => 3,
                "monto" => 4,
                "total" => 5,
                "corte" => 6,
                _ => 6
            };
        }

        private static int Cpr_PC_Planning_ContableSortColumn_Resolver(string? sortField)
        {
            return sortField?.Trim().ToLowerInvariant() switch
            {
                "cuenta" => 1,
                "descripcion" => 2,
                "unidad" => 3,
                "centro_costo" => 4,
                "total" => 5,
                "corte" => 6,
                _ => 6
            };
        }

        private static int Cpr_PC_Planning_SortOrder_Resolver(int? sortOrder)
        {
            return sortOrder == 1 ? 1 : -1;
        }

        private static ErrorDto<TLista> ToPagedListResponse<TRow, TDto, TLista>(
            ErrorDto<List<TRow>> r,
            Func<TRow, int> totalRowsSelector,
            Func<TRow, TDto> mapRow,
            Func<int, List<TDto>, TLista> listFactory,
            Func<TLista> emptyFactory)
        {
            var code = r.Code is int c ? c : -1;
            if (code != 0)
            {
                return DbHelper.CreateErrorResponse<TLista>(
                    r.Description ?? DefaultErrorDescription,
                    code,
                    emptyFactory());
            }

            var rows = r.Result ?? new List<TRow>();
            var total = rows.Count == 0 ? 0 : totalRowsSelector(rows[0]);
            var lineas = rows.Select(mapRow).ToList();

            return DbHelper.CreateOkResponse(listFactory(total, lineas));
        }


        private static CprPlanDTUpsert DeserializePlanDT(string parametros)
        {
            try
            {
                return JsonConvert.DeserializeObject<CprPlanDTUpsert>(parametros) ?? new CprPlanDTUpsert();
            }
            catch
            {
                return new CprPlanDTUpsert();
            }
        }

        private static (int CantidadTotal, decimal MontoTotal) CalcularTotales(List<CprPlanDTCortesDto>? cortes)
        {
            if (cortes == null || cortes.Count == 0)
                return (0, 0m);

            var cantidadTotal = 0;
            var montoTotal = 0m;

            foreach (var item in cortes)
            {
                cantidadTotal += item.cantidad;
                montoTotal += item.monto * item.cantidad;
            }

            return (cantidadTotal, montoTotal);
        }

        private ErrorDto<int> EjecutarPlanDtUpsert(int codEmpresa, CprPlanDTUpsert plan, int cantidadTotal, decimal montoTotal)
        {
            const string spSql = @"exec spCPR_Plan_DT_Upsert @IdPC, @CodProducto, @MontoUnitario, @CantidadTotal, @MontoTotal, @Usuario;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                spSql,
                0,
                new
                {
                    IdPC = plan.id_pc,
                    CodProducto = plan.cod_producto,
                    MontoUnitario = plan.monto_unitario,
                    CantidadTotal = cantidadTotal,
                    MontoTotal = montoTotal,
                    Usuario = plan.usuario
                }
            );
        }

        private ErrorDto<int> ObtenerConteoCortes(int codEmpresa, int idPlan)
        {
            const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
            return DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, existeSql, 0, new { id_plan = idPlan });
        }

        private ErrorDto GuardarCorte(CorteUpsertContext ctx, CprPlanDTCortesDto item)
        {
            if (ctx.Actualizar)
            {
                const string upd = @"
                                UPDATE CPR_PLAN_DT_CORTES
                                   SET CANTIDAD = @cantidad,
                                       MONTO = @monto,
                                       MODIFICA_FECHA = GETDATE(),
                                       MODIFICA_USUARIO = @usuario
                                 WHERE ID_PLAN = @id_plan AND CORTE = @corte;";

                var ur = DbHelper.ExecuteNonQuery(_portalDB, ctx.CodEmpresa, upd, new
                {
                    cantidad = item.cantidad,
                    monto = item.monto,
                    usuario = ctx.Usuario,
                    id_plan = ctx.IdPlan,
                    corte = item.corte
                });

                if (ur.Code == 0)
                    return DbHelper.CreateOkResponse();

                var uc = ur.Code is int c ? c : -1;
                return DbHelper.ErrorResponse(ur.Description ?? DefaultErrorDescription, uc);
            }

            const string ins = @"
                                INSERT INTO CPR_PLAN_DT_CORTES (CORTE, ID_PLAN, CANTIDAD, MONTO, REGISTRO_FECHA, REGISTRO_USUARIO)
                                VALUES (@corte, @id_plan, @cantidad, @monto, GETDATE(), @usuario);";

            var ir = DbHelper.ExecuteNonQuery(_portalDB, ctx.CodEmpresa, ins, new
            {
                corte = item.corte,
                id_plan = ctx.IdPlan,
                cantidad = item.cantidad,
                monto = item.monto,
                usuario = ctx.Usuario
            });

            if (ir.Code == 0)
                return DbHelper.CreateOkResponse();

            var ic2 = ir.Code is int c2 ? c2 : -1;
            return DbHelper.ErrorResponse(ir.Description ?? DefaultErrorDescription, ic2);
        }

        public ErrorDto<List<CprPlanComprasDto>> CprPlanCompras_Obtener(int CodEmpresa)
        {
            const string sql = @"SELECT * FROM CPR_PLAN_COMPRAS;";

            var r = DbHelper.ExecuteListQuery<CprPlanComprasDto>(_portalDB, CodEmpresa, sql, null);
            var code = r.Code is int c ? c : -1;

            if (code != 0)
                return new ErrorDto<List<CprPlanComprasDto>> { Code = code, Description = r.Description ?? DefaultErrorDescription, Result = null };

            return DbHelper.CreateOkResponse(r.Result ?? new List<CprPlanComprasDto>());
        }

        public ErrorDto<CprPlanDTDto> CprPlanDT_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto;";
            var existeR = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, existeSql, 0, new { id_pc = PlanCompras, cod_producto = CodProducto });
            var existeCode = existeR.Code is int ec ? ec : -1;
            if (existeCode != 0)
                return DbHelper.CreateErrorResponse<CprPlanDTDto>(
                    existeR.Description ?? DefaultErrorDescription,
                    existeCode,
                    new CprPlanDTDto());

            if (existeR.Result <= 0)
                return DbHelper.CreateErrorResponse<CprPlanDTDto>("Producto sin registrar", -1, new CprPlanDTDto());

            const string planSql = @"SELECT * FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto;";
            var planR = DbHelper.ExecuteSingleQuery<CprPlanDTDto>(_portalDB, CodEmpresa, planSql, null, new { id_pc = PlanCompras, cod_producto = CodProducto });
            var planCode = planR.Code is int pc ? pc : -1;
            if (planCode != 0)
                return DbHelper.CreateErrorResponse<CprPlanDTDto>(
                    planR.Description ?? DefaultErrorDescription,
                    planCode,
                    new CprPlanDTDto());

            var plan = planR.Result;
            if (plan == null)
                return DbHelper.CreateErrorResponse<CprPlanDTDto>("No se pudo obtener la información del producto.", -1, new CprPlanDTDto());

            const string uenSql = @"
                        SELECT DISTINCT
                            CASE
                                WHEN ISNULL(PC.COD_UNIDAD_DESTINO,'') = '' THEN PC.COD_UNIDAD
                                ELSE PC.COD_UNIDAD_DESTINO
                            END AS UEN
                        FROM CPR_PLAN_COMPRAS PC
                        WHERE ID_PC = @id_pc;";

            var uenR = DbHelper.ExecuteSingleQuery<string>(_portalDB, CodEmpresa, uenSql, string.Empty, new { id_pc = PlanCompras });
            var uen = uenR.Code == 0 ? (uenR.Result ?? string.Empty) : string.Empty;

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

            var totR = DbHelper.ExecuteSingleQuery<CprPlanDTTotalesData>(
                _portalDB,
                CodEmpresa,
                totalesSql,
                null,
                new { cod_producto = CodProducto, uen }
            );

            if (totR.Code == 0 && totR.Result != null)
            {
                plan.cantidad_transito = totR.Result.qty_solicitada;
                plan.cantidad_reservada = totR.Result.qty_recervada;
                plan.cantidad_despachada = totR.Result.qty_entregada;
            }
            else
            {
                plan.cantidad_transito = 0;
                plan.cantidad_reservada = 0;
                plan.cantidad_despachada = 0;
            }

            return DbHelper.CreateOkResponse(plan);
        }

        public ErrorDto<List<CprPlanDTCortesDto>> CprPlanDTCortes_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            const string idPlanSql = @"
                        SELECT COALESCE((SELECT ID_PLAN FROM CPR_PLAN_DT WHERE ID_PC = @id_pc AND COD_PRODUCTO = @cod_producto), 0) AS ID_PLAN;";

            var idPlanR = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, idPlanSql, 0, new { id_pc = PlanCompras, cod_producto = CodProducto });
            var idPlanCode = idPlanR.Code is int c ? c : -1;
            if (idPlanCode != 0)
                return new ErrorDto<List<CprPlanDTCortesDto>> { Code = idPlanCode, Description = idPlanR.Description ?? DefaultErrorDescription, Result = null };

            var idPlan = idPlanR.Result;

            if (idPlan != 0)
            {
                const string existeSql = @"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
                var exR = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, existeSql, 0, new { id_plan = idPlan });
                if (exR.Code == 0 && exR.Result > 0)
                {
                    const string cortesSql = @"SELECT corte, cantidad, monto FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = @id_plan;";
                    var listR = DbHelper.ExecuteListQuery<CprPlanDTCortesDto>(_portalDB, CodEmpresa, cortesSql, new { id_plan = idPlan });
                    var listCode = listR.Code is int lc ? lc : -1;
                    if (listCode != 0)
                        return new ErrorDto<List<CprPlanDTCortesDto>> { Code = listCode, Description = listR.Description ?? DefaultErrorDescription, Result = null };

                    return DbHelper.CreateOkResponse(listR.Result ?? new List<CprPlanDTCortesDto>());
                }
            }

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

            var genR = DbHelper.ExecuteListQuery<CprPlanDTCortesDto>(_portalDB, CodEmpresa, dateRangeSql, new { id_pc = PlanCompras });
            var genCode = genR.Code is int gc ? gc : -1;
            if (genCode != 0)
                return new ErrorDto<List<CprPlanDTCortesDto>> { Code = genCode, Description = genR.Description ?? DefaultErrorDescription, Result = null };

            return DbHelper.CreateOkResponse(genR.Result ?? new List<CprPlanDTCortesDto>());
        }

        public ErrorDto CprPlanCompras_Insert(int CodEmpresa, CprPlanComprasDto request)
        {
            const string sql = @"
                        INSERT INTO CPR_PLAN_COMPRAS (ID_PERIODO, COD_UNIDAD, COD_UNIDAD_DESTINO, ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES (@id_periodo, @cod_unidad, @cod_unidad_destino, 'P', GETDATE(), @registro_usuario);";

            var r = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
            {
                request.id_periodo,
                request.cod_unidad,
                request.cod_unidad_destino,
                request.registro_usuario
            });

            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse("Plan de compras agregado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? DefaultErrorDescription, code);
        }

        public ErrorDto CprPlanCompras_Update(int CodEmpresa, CprPlanComprasDto request)
        {
            const string sql = @"
                        UPDATE CPR_PLAN_COMPRAS
                           SET ID_PERIODO = @id_periodo,
                               COD_UNIDAD = @cod_unidad,
                               COD_UNIDAD_DESTINO = @cod_unidad_destino,
                               MODIFICA_FECHA = GETDATE(),
                               MODIFICA_USUARIO = @modifica_usuario
                         WHERE ID_PC = @id_pc;";

            var r = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
            {
                request.id_periodo,
                request.cod_unidad,
                request.cod_unidad_destino,
                request.modifica_usuario,
                request.id_pc
            });

            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse("Plan de compras actualizado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? DefaultErrorDescription, code);
        }

        public ErrorDto CprPlanDT_Upsert(int CodEmpresa, string parametros, List<CprPlanDTCortesDto> cortes)
        {
            try
            {
                var plan = DeserializePlanDT(parametros);
                var (cantidadTotal, montoTotal) = CalcularTotales(cortes);

                var idPlanR = EjecutarPlanDtUpsert(CodEmpresa, plan, cantidadTotal, montoTotal);
                var idPlanCode = CodeOrMinus1(idPlanR);
                if (idPlanCode != 0)
                    return DbHelper.ErrorResponse(idPlanR.Description ?? DefaultErrorDescription, idPlanCode);

                var idPlan = idPlanR.Result;

                var exR = ObtenerConteoCortes(CodEmpresa, idPlan);
                var exCode = CodeOrMinus1(exR);
                if (exCode != 0)
                    return DbHelper.ErrorResponse(exR.Description ?? DefaultErrorDescription, exCode);

                var ctx = new CorteUpsertContext(CodEmpresa, idPlan, plan.usuario ?? string.Empty, exR.Result > 0);
                foreach (var item in cortes ?? new List<CprPlanDTCortesDto>())
                {
                    var r = GuardarCorte(ctx, item);
                    if (r.Code != 0)
                        return r;
                }

                return DbHelper.OkResponse("Plan actualizado satisfactoriamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_Obtener(int CodEmpresa, string parametros)
        {
            var filtros = ParsePlanFiltros(parametros);
            var args = BuildPagedArgs(filtros, includeCorte: true);

            return ObtenerResumenPlanCore(CodEmpresa, filtros, args, prodClas: null);
        }

        private ErrorDto<CprResumenPlanLista> ObtenerResumenPlanCore(int codEmpresa, CprPlanFiltros filtros, PagedArgs args, int? prodClas)
        {
            var sortColumn = Cpr_PC_Planning_ResumenSortColumn_Resolver(filtros.sortField);
            var sortOrder = Cpr_PC_Planning_SortOrder_Resolver(filtros.sortOrder);

            const string sql = @"
                        SELECT D.COD_PRODUCTO,
                               P.DESCRIPCION,
                               S.CANTIDAD,
                               S.MONTO,
                               S.CORTE,
                               COUNT(*) OVER() AS TotalRows
                          FROM CPR_PLAN_DT D
                          INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                          INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                          INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                         WHERE D.ID_PC = @IdPc
                           AND (@Corte IS NULL OR CONVERT(varchar(10), S.CORTE, 111) = @Corte)
                           AND (@Q IS NULL OR (D.COD_PRODUCTO LIKE @Q OR P.DESCRIPCION LIKE @Q))
                           AND (@ProdClas IS NULL OR P.COD_PRODCLAS = @ProdClas)
                         ORDER BY
                               CASE WHEN @SortColumn = 1 AND @SortOrder = 1 THEN D.COD_PRODUCTO END ASC,
                               CASE WHEN @SortColumn = 1 AND @SortOrder = -1 THEN D.COD_PRODUCTO END DESC,
                               CASE WHEN @SortColumn = 2 AND @SortOrder = 1 THEN P.DESCRIPCION END ASC,
                               CASE WHEN @SortColumn = 2 AND @SortOrder = -1 THEN P.DESCRIPCION END DESC,
                               CASE WHEN @SortColumn = 3 AND @SortOrder = 1 THEN S.CANTIDAD END ASC,
                               CASE WHEN @SortColumn = 3 AND @SortOrder = -1 THEN S.CANTIDAD END DESC,
                               CASE WHEN @SortColumn = 4 AND @SortOrder = 1 THEN S.MONTO END ASC,
                               CASE WHEN @SortColumn = 4 AND @SortOrder = -1 THEN S.MONTO END DESC,
                               CASE WHEN @SortColumn = 5 AND @SortOrder = 1 THEN S.MONTO * S.CANTIDAD END ASC,
                               CASE WHEN @SortColumn = 5 AND @SortOrder = -1 THEN S.MONTO * S.CANTIDAD END DESC,
                               CASE WHEN @SortColumn = 6 AND @SortOrder = 1 THEN S.CORTE END ASC,
                               CASE WHEN @SortColumn = 6 AND @SortOrder = -1 THEN S.CORTE END DESC,
                               S.CORTE DESC,
                               D.COD_PRODUCTO ASC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<ResumenPlanRow>(_portalDB, codEmpresa, sql, new
            {
                IdPc = filtros.planCompras,
                Corte = args.Corte,
                Q = args.Q,
                ProdClas = prodClas,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Offset = args.Offset,
                Fetch = args.Fetch
            });

            return ToPagedListResponse(
                r,
                row => row.TotalRows,
                x => new CprResumenPlanDto
                {
                    cod_producto = x.COD_PRODUCTO,
                    descripcion = x.DESCRIPCION,
                    cantidad = x.CANTIDAD,
                    monto = x.MONTO,
                    corte = x.CORTE
                },
                (total, lineas) => new CprResumenPlanLista { Total = total, Lineas = lineas },
                () => new CprResumenPlanLista { Total = 0, Lineas = new List<CprResumenPlanDto>() }
            );
        }

        public ErrorDto<CprPlanContableLista> CprPlanContable_Obtener(int CodEmpresa, string parametros)
        {
            var filtros = ParsePlanFiltros(parametros);
            var args = BuildPagedArgs(filtros, includeCorte: true);
            var sortColumn = Cpr_PC_Planning_ContableSortColumn_Resolver(filtros.sortField);
            var sortOrder = Cpr_PC_Planning_SortOrder_Resolver(filtros.sortOrder);

            const string sql = @"
                        WITH PlanContable AS
                        (
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
                               AND (@Corte IS NULL OR CONVERT(varchar(10), S.CORTE, 111) = @Corte)
                               AND (@Q IS NULL OR (Z.COD_CUENTA_MASK LIKE @Q OR Z.DESCRIPCION LIKE @Q))
                        )
                        SELECT CUENTA,
                               DESCRIPCION,
                               UNIDAD,
                               CENTRO_COSTO,
                               TOTAL,
                               CORTE,
                               COUNT(*) OVER() AS TotalRows
                          FROM PlanContable
                         ORDER BY
                               CASE WHEN @SortColumn = 1 AND @SortOrder = 1 THEN CUENTA END ASC,
                               CASE WHEN @SortColumn = 1 AND @SortOrder = -1 THEN CUENTA END DESC,
                               CASE WHEN @SortColumn = 2 AND @SortOrder = 1 THEN DESCRIPCION END ASC,
                               CASE WHEN @SortColumn = 2 AND @SortOrder = -1 THEN DESCRIPCION END DESC,
                               CASE WHEN @SortColumn = 3 AND @SortOrder = 1 THEN UNIDAD END ASC,
                               CASE WHEN @SortColumn = 3 AND @SortOrder = -1 THEN UNIDAD END DESC,
                               CASE WHEN @SortColumn = 4 AND @SortOrder = 1 THEN CENTRO_COSTO END ASC,
                               CASE WHEN @SortColumn = 4 AND @SortOrder = -1 THEN CENTRO_COSTO END DESC,
                               CASE WHEN @SortColumn = 5 AND @SortOrder = 1 THEN TOTAL END ASC,
                               CASE WHEN @SortColumn = 5 AND @SortOrder = -1 THEN TOTAL END DESC,
                               CASE WHEN @SortColumn = 6 AND @SortOrder = 1 THEN CORTE END ASC,
                               CASE WHEN @SortColumn = 6 AND @SortOrder = -1 THEN CORTE END DESC,
                               CORTE DESC,
                               CUENTA ASC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<PlanContableRow>(_portalDB, CodEmpresa, sql, new
            {
                IdPc = filtros.planCompras,
                Corte = args.Corte,
                Q = args.Q,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Offset = args.Offset,
                Fetch = args.Fetch
            });

            return ToPagedListResponse(
                r,
                row => row.TotalRows,
                x => new CprPlanContableDto
                {
                    cuenta = x.CUENTA,
                    descripcion = x.DESCRIPCION,
                    unidad = x.UNIDAD,
                    centro_costo = x.CENTRO_COSTO,
                    total = x.TOTAL,
                    corte = x.CORTE
                },
                (total, lineas) => new CprPlanContableLista { Total = total, Lineas = lineas },
                () => new CprPlanContableLista { Total = 0, Lineas = new List<CprPlanContableDto>() }
            );
        }

        public ErrorDto<CprBitacoraLista> CprBitacora_Obtener(int CodEmpresa, string parametros)
        {
            var filtros = ParsePlanFiltros(parametros);
            var args = BuildPagedArgs(filtros, includeCorte: false);

            var mov = $"%Plan:{filtros.planCompras}%";

            const string sql = @"
                        SELECT ID_BITACORA,
                               FECHAHORA,
                               USUARIO,
                               DETALLE,
                               COUNT(*) OVER() AS TotalRows
                          FROM CPR_BITACORA_SOLICITUD
                         WHERE MOVIMIENTO LIKE @Mov
                           AND (@Q IS NULL OR (USUARIO LIKE @Q OR DETALLE LIKE @Q OR CONVERT(VARCHAR(30), FECHAHORA, 120) LIKE @Q))
                         ORDER BY FECHAHORA DESC
                         OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var r = DbHelper.ExecuteListQuery<BitacoraRow>(_portalDB, CodEmpresa, sql, new
            {
                Mov = mov,
                Q = args.Q,
                Offset = args.Offset,
                Fetch = args.Fetch
            });

            return ToPagedListResponse(
                r,
                row => row.TotalRows,
                x => new CprBitacoraDto
                {
                    id_bitacora = x.ID_BITACORA,
                    fechahora = x.FECHAHORA,
                    usuario = x.USUARIO,
                    detalle = x.DETALLE
                },
                (total, lineas) => new CprBitacoraLista { Total = total, Lineas = lineas },
                () => new CprBitacoraLista { Total = 0, Lineas = new List<CprBitacoraDto>() }
            );
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_ObtenerxCuenta(int CodEmpresa, string parametros)
        {
            var filtros = ParsePlanFiltros(parametros);

            const string prodClasSql = @"SELECT TOP 1 COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE COD_CUENTA = @cod_cuenta;";
            var prodR = DbHelper.ExecuteSingleQuery<int?>(_portalDB, CodEmpresa, prodClasSql, 0, new { cod_cuenta = filtros.filtro });
            var prodclas = (prodR.Code == 0 ? (prodR.Result ?? 0) : 0);

            if (prodclas <= 0)
                return DbHelper.CreateOkResponse(new CprResumenPlanLista { Total = 0, Lineas = new List<CprResumenPlanDto>() });

            var args = BuildPagedArgs(filtros, includeCorte: true);
            args = args with { Q = null };

            return ObtenerResumenPlanCore(CodEmpresa, filtros, args, prodClas: prodclas);
        }

        public ErrorDto<string> CprPlanCompras_PlanesEstrategicos(int codEmpresa, int planCompras)
        {
            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                codEmpresa,
                "exec spPE_W_GetPlanificacionTree @CPR_ID",
                "[]",
                new { CPR_ID = planCompras });

            var code = result.Code is int c ? c : -1;
            if (code != 0)
            {
                return DbHelper.CreateErrorResponse<string>(
                    result.Description ?? DefaultErrorDescription,
                    code,
                    string.Empty);
            }

            return DbHelper.CreateOkResponse(result.Result ?? "[]");
        }

        public ErrorDto<int> CprPlanCompras_AgregarSeleccion(int codEmpresa, List<CprSeleccionDto> planEst)
        {
            if (planEst == null || planEst.Count == 0)
            {
                return DbHelper.CreateErrorResponse<int>("Debe seleccionar al menos un elemento.", -1, 0);
            }

            var cprId = planEst[0].cprId;
            if (cprId <= 0)
            {
                return DbHelper.CreateErrorResponse<int>("El plan de compras no es válido.", -1, 0);
            }

            var tabla = Cpr_PC_Planning_Seleccion_DataTable_Crear();
            Cpr_PC_Planning_Seleccion_DataTable_Llenar(planEst, tabla);

            var execResult = DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                using var cmd = new SqlCommand("spPE_W_InsertPlanComprasSeleccion_Tvp", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CPR_ID", cprId);
                var pItems = cmd.Parameters.AddWithValue("@Items", tabla);
                pItems.SqlDbType = SqlDbType.Structured;
                pItems.TypeName = "dbo.CprSeleccionItem";

                cmd.ExecuteNonQuery();
                return 1;
            });

            var execCode = execResult.Code is int ec ? ec : -1;
            if (execCode != 0)
            {
                return DbHelper.CreateErrorResponse<int>(execResult.Description ?? DefaultErrorDescription, execCode, 0);
            }

            return DbHelper.CreateOkResponse(1, "OK");
        }

        private static DataTable Cpr_PC_Planning_Seleccion_DataTable_Crear()
        {
            var dt = new DataTable();
            dt.Columns.Add("cprId", typeof(int));
            dt.Columns.Add("NodoTipo", typeof(string));
            dt.Columns.Add("PeId", typeof(int));
            dt.Columns.Add("PerspectivaId", typeof(int));
            dt.Columns.Add("ProyectoId", typeof(int));
            dt.Columns.Add("ObjetivoId", typeof(int));
            return dt;
        }

        private static void Cpr_PC_Planning_Seleccion_DataTable_Llenar(List<CprSeleccionDto> planEst, DataTable tabla)
        {
            foreach (var item in planEst)
            {
                tabla.Rows.Add(
                    item.cprId,
                    item.nodoTipo,
                    item.peId.HasValue ? item.peId.Value : DBNull.Value,
                    item.perspectivaId.HasValue ? item.perspectivaId.Value : DBNull.Value,
                    item.proyectoId.HasValue ? item.proyectoId.Value : DBNull.Value,
                    item.objetivoId.HasValue ? item.objetivoId.Value : DBNull.Value);
            }
        }
    }
}
