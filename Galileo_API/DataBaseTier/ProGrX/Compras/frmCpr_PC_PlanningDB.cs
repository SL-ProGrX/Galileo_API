using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_PC_PlanningDB
    {
        private readonly IConfiguration _config;

        public frmCpr_PC_PlanningDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<cprPlanComprasDTO>> CprPlanCompras_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<cprPlanComprasDTO>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT * FROM CPR_PLAN_COMPRAS";
                    response.Result = connection.Query<cprPlanComprasDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO<cprPlanDTDTO> CprPlanDT_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<cprPlanDTDTO>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COUNT(*) FROM CPR_PLAN_DT WHERE ID_PC = {PlanCompras} AND COD_PRODUCTO = '{CodProducto}'";
                    int existe = connection.Query<int>(query).FirstOrDefault();
                    if (existe > 0){
                        query = $@"SELECT * FROM CPR_PLAN_DT WHERE ID_PC = {PlanCompras} AND COD_PRODUCTO = '{CodProducto}'";
                        response.Result = connection.Query<cprPlanDTDTO>(query).FirstOrDefault();

                        //Busco la unidad correspondiente
                        query = $@"SELECT DISTINCT CASE 
                                        WHEN PC.COD_UNIDAD_DESTINO = '' THEN PC.COD_UNIDAD 
                                        ELSE PC.COD_UNIDAD_DESTINO
                                    END AS UEN FROM CPR_PLAN_COMPRAS PC WHERE ID_PC = {PlanCompras} ";
                        string UEN = connection.Query<string>(query).FirstOrDefault();

                        //Busco cantidades en transito (en ordenes de Compra por solicitud.)
                        query = $@"SELECT T.*, 
                                    ( SELECT COUNT(A.COD_PRODUCTO) FROM PV_CONTROL_ACTIVOS A 
                                    WHERE A.COD_UEN = T.UEN AND A.COD_PRODUCTO = T.COD_PRODUCTO 
                                    AND A.ENTREGA_USUARIO = ''
                                    ) 
                                    AS QTY_RECERVADA,
                                    ( SELECT COUNT(A.COD_PRODUCTO) FROM PV_CONTROL_ACTIVOS A 
                                    WHERE A.COD_UEN = T.UEN AND A.COD_PRODUCTO = T.COD_PRODUCTO 
                                    AND A.ENTREGA_USUARIO != ''
                                    ) 
                                    AS QTY_ENTREGADA
                                    FROM 
                                    (
                                    SELECT DISTINCT 
                                    PC.ID_PC, SP.ADJUDICA_ORDEN, D.COD_PRODUCTO, 
                                    CASE 
                                                WHEN PC.COD_UNIDAD_DESTINO = '' THEN PC.COD_UNIDAD 
                                                ELSE PC.COD_UNIDAD_DESTINO
                                            END AS UEN
                                    ,
                                    D.CANTIDAD AS QTY_SOLICITADA, 
                                    (  
                                    SELECT CANTIDAD_TOTAL FROM CPR_PLAN_DT P WHERE COD_PRODUCTO = '{CodProducto}'
                                    AND P.ID_PC = PC.ID_PC
                                    ) AS QTY_PLAN_COMPRAS 
                                    FROM CPR_ORDENES_DETALLE D 
                                    LEFT JOIN CPR_SOLICITUD_PROV SP ON SP.ADJUDICA_ORDEN = D.COD_ORDEN 
                                    LEFT JOIN CPR_SOLICITUD_BS SB ON  SP.CPR_ID = SB.CPR_ID 
                                    LEFT JOIN CPR_PLAN_COMPRAS PC ON PC.COD_UNIDAD = SB.COD_UNIDAD 
                                    WHERE D.COD_PRODUCTO = '{CodProducto}' AND PC.ID_PC IN (
                                    SELECT ID_PC FROM CPR_PLAN_DT WHERE COD_PRODUCTO = '{CodProducto}'
                                    ))T WHERE T.UEN = '{UEN}' ";
                        var totales = connection.Query<cprPlanDTTotalesData>(query).FirstOrDefault();

                        if(totales != null)
                        {
                            response.Result.cantidad_transito = totales.qty_solicitada;
                            response.Result.cantidad_reservada = totales.qty_recervada;
                            response.Result.cantidad_despachada = totales.qty_entregada;
                        }
                        else
                        {
                            response.Result.cantidad_transito = 0;
                            response.Result.cantidad_reservada = 0;
                            response.Result.cantidad_despachada = 0;
                        }
                        




                    } else {
                        response.Description = "Producto sin registrar";
                        response.Result = null;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO<List<cprPlanDTCortesDTO>> CprPlanDTCortes_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<cprPlanDTCortesDTO>>();
            response.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //Valide si existe algún registro
                    var query = $"SELECT COALESCE((SELECT ID_PLAN FROM CPR_PLAN_DT WHERE ID_PC = {PlanCompras} AND COD_PRODUCTO = '{CodProducto}'), 0) AS ID_PLAN";
                    int idPlan = connection.Query<int>(query).FirstOrDefault();

                    query = $"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = {idPlan}";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (idPlan != 0 && existe > 0)
                    {
                        query = $@"SELECT corte, cantidad, monto FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = {idPlan}";
                    }
                    else
                    {
                        query = $@"WITH DateRange AS ( 
                                SELECT 
                                    CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, P.INICIO) + 1, -1)) AS corte
                                FROM 
                                    CPR_PLAN_PERIODOS P
                                INNER JOIN 
                                    CPR_PLAN_COMPRAS C ON P.ID_PERIODO = C.ID_PERIODO
                                WHERE 
                                    C.ID_PC = {PlanCompras}
                                UNION ALL
                                SELECT 
                                    CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, DATEADD(MONTH, 1, corte)) + 1, -1))
                                FROM 
                                    DateRange
                                WHERE 
                                    DATEADD(MONTH, 1, corte) <= (
                                        SELECT 
                                            CONVERT(DATE, DATEADD(MONTH, DATEDIFF(MONTH, 0, CORTE) + 1, -1))
                                        FROM 
                                            CPR_PLAN_PERIODOS P
                                        INNER JOIN 
                                            CPR_PLAN_COMPRAS C ON P.ID_PERIODO = C.ID_PERIODO
                                        WHERE 
                                            C.ID_PC = {PlanCompras}
                                    )
                            )
                            SELECT 
                                corte, 
                                0 AS cantidad, 
                                0 AS monto
                            FROM 
                                DateRange
                            ORDER BY 
                                corte;";
                    }
                    response.Result = connection.Query<cprPlanDTCortesDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO CprPlanCompras_Insert(int CodEmpresa, cprPlanComprasDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"insert into CPR_PLAN_COMPRAS(ID_PERIODO, COD_UNIDAD, COD_UNIDAD_DESTINO ,ESTADO, REGISTRO_FECHA, REGISTRO_USUARIO) 
                            values('{request.id_periodo}','{request.cod_unidad}', '{request.cod_unidad_destino}' ,'P', Getdate(), '{request.registro_usuario}' )";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Plan de compras agregado satisfactoriamente";
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDTO CprPlanCompras_Update(int CodEmpresa, cprPlanComprasDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"update CPR_PLAN_COMPRAS set ID_PERIODO = '{request.id_periodo}',
                            COD_UNIDAD = '{request.cod_unidad}',
                            COD_UNIDAD_DESTINO = '{request.cod_unidad_destino}',
                            Modifica_Fecha = Getdate(), Modifica_Usuario = '{request.modifica_usuario}'
                            where ID_PC = {request.id_pc}";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Plan de compras actualizado satisfactoriamente";
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDTO CprPlanDT_Upsert(int CodEmpresa, string parametros, List<cprPlanDTCortesDTO> cortes)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var plan = JsonConvert.DeserializeObject<cprPlanDTUpsert>(parametros);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                int cantidadTotal = 0;
                decimal montoTotal = 0;
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in cortes)
                    {
                        cantidadTotal += item.cantidad;
                        montoTotal += item.monto * item.cantidad;
                    }
                    var query = @$"exec spCPR_Plan_DT_Upsert @IdPC, @CodProducto, @MontoUnitario, @CantidadTotal, @MontoTotal, @Usuario";

                    var parameters = new DynamicParameters();
                    parameters.Add("IdPC", plan.id_pc, DbType.Int32);
                    parameters.Add("CodProducto", plan.cod_producto, DbType.String);
                    parameters.Add("MontoUnitario", plan.monto_unitario, DbType.Decimal);
                    parameters.Add("CantidadTotal", cantidadTotal, DbType.Int32);
                    parameters.Add("MontoTotal", montoTotal, DbType.Decimal);
                    parameters.Add("Usuario", plan.usuario, DbType.String);

                    int Id_Plan = connection.Query<int>(query, parameters).FirstOrDefault();

                    query = $"SELECT COUNT(*) FROM CPR_PLAN_DT_CORTES WHERE ID_PLAN = {Id_Plan}";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    foreach (var item in cortes)
                    {
                        if (existe > 0)
                        {
                            query = @$"update CPR_PLAN_DT_CORTES set CANTIDAD = {item.cantidad}, monto = {item.monto},
                            Modifica_Fecha = Getdate(), Modifica_Usuario = '{plan.usuario}'
                            where ID_PLAN = {Id_Plan} AND CORTE = '{item.corte}'";
                        }
                        else
                        {
                            query = @$"insert into CPR_PLAN_DT_CORTES(CORTE, ID_PLAN, CANTIDAD, MONTO, REGISTRO_FECHA, REGISTRO_USUARIO) 
                            values('{item.corte}', {Id_Plan}, {item.cantidad}, {item.monto}, Getdate(), '{plan.usuario}' )";
                        }
                        resp.Code = connection.ExecuteAsync(query).Result;
                    }

                    resp.Description = "Plan actualizado satisfactoriamente";
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDTO<CprResumenPlanLista> CprResumenPlan_Obtener(int CodEmpresa, string parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            cprPlanFiltros filtros = JsonConvert.DeserializeObject<cprPlanFiltros>(parametros) ?? new cprPlanFiltros();
            var response = new ErrorDTO<CprResumenPlanLista>();
            response.Result = new CprResumenPlanLista();
            response.Code = 0;
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                string where = $"WHERE  D.ID_PC = {filtros.planCompras} ";
                if (filtros.periodo != "Todos")
                {
                    where += $"AND S.CORTE = '{filtros.periodo}'";
                }
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += "AND D.COD_PRODUCTO LIKE '%" + filtros.filtro + "%' OR P.DESCRIPCION LIKE '%" + filtros.filtro + "%' ";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COUNT(D.COD_PRODUCTO) from CPR_PLAN_DT D 
                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC 
                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN 
                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO 
                        {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select D.COD_PRODUCTO, P.DESCRIPCION, S.CANTIDAD, S.MONTO, S.CORTE from CPR_PLAN_DT D
                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                        {where} order by S.CORTE desc {paginaActual} {paginacionActual}";
                    response.Result.Lineas = connection.Query<cprResumenPlanDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Lineas = null;
                response.Result.Total = 0;
            }

            return response;
        }

        public ErrorDTO<cprPlanContableLista> CprPlanContable_Obtener(int CodEmpresa, string parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var filtros = JsonConvert.DeserializeObject<cprPlanFiltros>(parametros);
            var response = new ErrorDTO<cprPlanContableLista>();
            response.Result = new cprPlanContableLista();
            response.Code = 0;
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                string where = $"WHERE  D.ID_PC = {filtros.planCompras} ";
                if (filtros.periodo != "Todos")
                {
                    where += $"AND S.CORTE = '{filtros.periodo}' ";
                }
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += "AND Z.COD_CUENTA_MASK LIKE '%" + filtros.filtro + "%' OR Z.DESCRIPCION LIKE '%" + filtros.filtro + "%' ";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COUNT(*) from (
                            select distinct Z.COD_CUENTA_MASK, Z.DESCRIPCION, S.CORTE from CPR_PLAN_DT D
                            INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                            INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                            INNER JOIN CORE_UENS U ON C.COD_UNIDAD = U.COD_UNIDAD
                            INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                            INNER JOIN PV_PROD_CLASIFICA B ON P.COD_PRODCLAS = B.COD_PRODCLAS
                            INNER JOIN CNTX_CUENTAS Z ON B.COD_CUENTA = Z.COD_CUENTA
                        {where}) T";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select distinct Z.COD_CUENTA_MASK AS CUENTA, Z.DESCRIPCION, U.CNTX_UNIDAD AS UNIDAD, U.CNTX_CENTRO_COSTO AS CENTRO_COSTO,
                            C.PRES_MONTO AS TOTAL, S.CORTE from CPR_PLAN_DT D
                            INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                            INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                            INNER JOIN CORE_UENS U ON C.COD_UNIDAD = U.COD_UNIDAD
                            INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                            INNER JOIN PV_PROD_CLASIFICA B ON P.COD_PRODCLAS = B.COD_PRODCLAS
                            INNER JOIN CNTX_CUENTAS Z ON B.COD_CUENTA = Z.COD_CUENTA
                        {where} order by S.CORTE desc {paginaActual} {paginacionActual}";
                    response.Result.Lineas = connection.Query<cprPlanContableDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code= -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO<cprBitacoraLista> CprBitacora_Obtener(int CodEmpresa, string parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            cprPlanFiltros filtros = JsonConvert.DeserializeObject<cprPlanFiltros>(parametros) ?? new cprPlanFiltros();
            var response = new ErrorDTO<cprBitacoraLista>();
            response.Result = new cprBitacoraLista();
            response.Code = 0;
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                string where = "WHERE MOVIMIENTO LIKE '%Plan:" + filtros.planCompras + "' ";
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += "AND USUARIO LIKE '%" + filtros.filtro + "%' OR DETALLE LIKE '%" + filtros.filtro + "%' " +
                        "OR FECHAHORA LIKE '%" + filtros.filtro + "%' ";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"SELECT COUNT(*) FROM CPR_BITACORA_SOLICITUD {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();
                    query = $@"SELECT ID_BITACORA, FECHAHORA, USUARIO, DETALLE FROM CPR_BITACORA_SOLICITUD 
                        {where} order by FECHAHORA desc {paginaActual} {paginacionActual}";
                    response.Result.Lineas = connection.Query<cprBitacoraDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO<CprResumenPlanLista> CprResumenPlan_ObtenerxCuenta(int CodEmpresa, string parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            cprPlanFiltros filtros = JsonConvert.DeserializeObject<cprPlanFiltros>(parametros) ?? new cprPlanFiltros();
            var response = new ErrorDTO<CprResumenPlanLista>();
            response.Result = new CprResumenPlanLista();
            response.Code = 0;
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                string where = $"AND D.ID_PC = {filtros.planCompras} ";
                if (filtros.periodo != "Todos")
                {
                    where += $"AND S.CORTE = '{filtros.periodo}'";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select * from PV_PROD_CLASIFICA where COD_CUENTA = '{filtros.filtro}'";
                    int prodclas = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select COUNT(D.COD_PRODUCTO) from CPR_PLAN_DT D 
                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC 
                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN 
                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO 
                        where P.COD_PRODCLAS = {prodclas} {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select D.COD_PRODUCTO, P.DESCRIPCION, S.CANTIDAD, S.MONTO, S.CORTE from CPR_PLAN_DT D
                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                        where P.COD_PRODCLAS = {prodclas} {where} 
                        order by S.CORTE desc {paginaActual} {paginacionActual}";
                    response.Result.Lineas = connection.Query<cprResumenPlanDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Lineas = null;
                response.Result.Total = 0;
            }

            return response;
        }
    }
}