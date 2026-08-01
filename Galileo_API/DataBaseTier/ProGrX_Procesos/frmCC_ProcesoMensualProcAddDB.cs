using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Procesos
{
    public class FrmCCProcesoMensualProcAddDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string CampoTransaccion = "TRANSACCION";
        private const string CampoProceso = "PROCESO";
        private const string CampoProcNum = "PROC_NUM";
        private const string CampoEjecucionTipo = "EJECUCION_TIPO";
        private const string CampoEjecucionOrden = "EJECUCION_ORDEN";
        private const string CampoProcedimiento = "PROCEDIMIENTO";
        private const string CampoDescripcion = "DESCRIPCION";
        private const string CampoParametrosPlanillas = "PARAMETROS_PLANILLAS";
        private const string CampoParametrosAdd = "PARAMETROS_ADD";
        private const string SqlProcesosComplementariosBase = @"SELECT
                  p.transaccion,
                  CASE
                    WHEN p.transaccion = '01' THEN 'Cambia Fecha de Proceso'
                    WHEN p.transaccion = '02' THEN 'Genera deducciones'
                    WHEN p.transaccion = '03' THEN 'Carga deducciones'
                    WHEN p.transaccion = '04' THEN 'Desglosa deducciones'
                    WHEN p.transaccion = '05' THEN 'Aplica Ahorros'
                    WHEN p.transaccion = '06' THEN 'Inconsistencias de Ahorros'
                    WHEN p.transaccion = '07' THEN 'Devoluciones de Ahorros'
                    WHEN p.transaccion = '08' THEN 'Aplica Abonos'
                    WHEN p.transaccion = '09' THEN 'Reporte de Inconsistencias'
                    WHEN p.transaccion = '10' THEN 'Actualiza Intereses Moratorios'
                    WHEN p.transaccion = '11' THEN 'Actualiza Saldo del Mes'
                    ELSE ''
                  END AS proceso,
                  p.proc_num,
                  p.ejecucion_tipo,
                  ISNULL(LTRIM(RTRIM(p.ejecucion_orden)), '') AS ejecucion_orden,
                  p.procedimiento,
                  p.descripcion,
                  CAST(p.parametros_planillas AS bit) AS parametros_planillas,
                  ISNULL(p.parametros_add,'') AS parametros_add
              FROM prm_procesos_add p
              WHERE (
                    @filtro IS NULL
                 OR p.transaccion LIKE '%' + @filtro + '%'
                 OR p.procedimiento LIKE '%' + @filtro + '%'
                 OR p.descripcion LIKE '%' + @filtro + '%'
                 OR p.ejecucion_tipo LIKE '%' + @filtro + '%'
                 OR p.parametros_add LIKE '%' + @filtro + '%'
                 OR p.ejecucion_orden LIKE '%' + @filtro + '%'
              )";
        private const string SqlProcesosComplementariosTotal = @"SELECT COUNT(1)
              FROM prm_procesos_add p
              WHERE (
                    @filtro IS NULL
                 OR p.transaccion LIKE '%' + @filtro + '%'
                 OR p.procedimiento LIKE '%' + @filtro + '%'
                 OR p.descripcion LIKE '%' + @filtro + '%'
                 OR p.ejecucion_tipo LIKE '%' + @filtro + '%'
                 OR p.parametros_add LIKE '%' + @filtro + '%'
                 OR p.ejecucion_orden LIKE '%' + @filtro + '%'
              )";

        public FrmCCProcesoMensualProcAddDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene lista paginada de procesos complementarios de planillas (PRM_PROCESOS_ADD) con filtros LazyLoad y ordenamiento por whitelist.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaProcesosComplementariosLista> CC_PlanillaProcesosComplementarios_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de procesos complementarios son requeridos.", -2, CrearResultadoListaVacio());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new DynamicParameters();
                parametros.Add("@filtro", string.IsNullOrWhiteSpace(filtros.filtro) ? null : filtros.filtro.Trim());
                parametros.Add("@SortField", NormalizarSortFieldProcesos(filtros.sortField));
                parametros.Add("@SortOrder", filtros.sortOrder == 2 ? "DESC" : "ASC");
                parametros.Add("@offset", filtros.pagina < 0 ? 0 : filtros.pagina);
                parametros.Add("@fetch", filtros.paginacion <= 0 ? 30 : filtros.paginacion);

                const string sql = SqlProcesosComplementariosBase + @"
              ORDER BY
                    CASE WHEN @SortField = 'TRANSACCION' AND @SortOrder = 'ASC' THEN p.transaccion END ASC,
                    CASE WHEN @SortField = 'TRANSACCION' AND @SortOrder = 'DESC' THEN p.transaccion END DESC,
                    CASE WHEN @SortField = 'PROCESO' AND @SortOrder = 'ASC' THEN CASE
                        WHEN p.transaccion = '01' THEN 'Cambia Fecha de Proceso'
                        WHEN p.transaccion = '02' THEN 'Genera deducciones'
                        WHEN p.transaccion = '03' THEN 'Carga deducciones'
                        WHEN p.transaccion = '04' THEN 'Desglosa deducciones'
                        WHEN p.transaccion = '05' THEN 'Aplica Ahorros'
                        WHEN p.transaccion = '06' THEN 'Inconsistencias de Ahorros'
                        WHEN p.transaccion = '07' THEN 'Devoluciones de Ahorros'
                        WHEN p.transaccion = '08' THEN 'Aplica Abonos'
                        WHEN p.transaccion = '09' THEN 'Reporte de Inconsistencias'
                        WHEN p.transaccion = '10' THEN 'Actualiza Intereses Moratorios'
                        WHEN p.transaccion = '11' THEN 'Actualiza Saldo del Mes'
                        ELSE ''
                    END END ASC,
                    CASE WHEN @SortField = 'PROCESO' AND @SortOrder = 'DESC' THEN CASE
                        WHEN p.transaccion = '01' THEN 'Cambia Fecha de Proceso'
                        WHEN p.transaccion = '02' THEN 'Genera deducciones'
                        WHEN p.transaccion = '03' THEN 'Carga deducciones'
                        WHEN p.transaccion = '04' THEN 'Desglosa deducciones'
                        WHEN p.transaccion = '05' THEN 'Aplica Ahorros'
                        WHEN p.transaccion = '06' THEN 'Inconsistencias de Ahorros'
                        WHEN p.transaccion = '07' THEN 'Devoluciones de Ahorros'
                        WHEN p.transaccion = '08' THEN 'Aplica Abonos'
                        WHEN p.transaccion = '09' THEN 'Reporte de Inconsistencias'
                        WHEN p.transaccion = '10' THEN 'Actualiza Intereses Moratorios'
                        WHEN p.transaccion = '11' THEN 'Actualiza Saldo del Mes'
                        ELSE ''
                    END END DESC,
                    CASE WHEN @SortField = 'PROC_NUM' AND @SortOrder = 'ASC' THEN p.proc_num END ASC,
                    CASE WHEN @SortField = 'PROC_NUM' AND @SortOrder = 'DESC' THEN p.proc_num END DESC,
                    CASE WHEN @SortField = 'EJECUCION_TIPO' AND @SortOrder = 'ASC' THEN p.ejecucion_tipo END ASC,
                    CASE WHEN @SortField = 'EJECUCION_TIPO' AND @SortOrder = 'DESC' THEN p.ejecucion_tipo END DESC,
                    CASE WHEN @SortField = 'EJECUCION_ORDEN' AND @SortOrder = 'ASC' THEN CASE
                        WHEN ISNUMERIC(LTRIM(RTRIM(p.ejecucion_orden))) = 1 THEN CAST(LTRIM(RTRIM(p.ejecucion_orden)) AS int)
                        ELSE 0
                    END END ASC,
                    CASE WHEN @SortField = 'EJECUCION_ORDEN' AND @SortOrder = 'DESC' THEN CASE
                        WHEN ISNUMERIC(LTRIM(RTRIM(p.ejecucion_orden))) = 1 THEN CAST(LTRIM(RTRIM(p.ejecucion_orden)) AS int)
                        ELSE 0
                    END END DESC,
                    CASE WHEN @SortField = 'PROCEDIMIENTO' AND @SortOrder = 'ASC' THEN p.procedimiento END ASC,
                    CASE WHEN @SortField = 'PROCEDIMIENTO' AND @SortOrder = 'DESC' THEN p.procedimiento END DESC,
                    CASE WHEN @SortField = 'DESCRIPCION' AND @SortOrder = 'ASC' THEN p.descripcion END ASC,
                    CASE WHEN @SortField = 'DESCRIPCION' AND @SortOrder = 'DESC' THEN p.descripcion END DESC,
                    CASE WHEN @SortField = 'PARAMETROS_PLANILLAS' AND @SortOrder = 'ASC' THEN p.parametros_planillas END ASC,
                    CASE WHEN @SortField = 'PARAMETROS_PLANILLAS' AND @SortOrder = 'DESC' THEN p.parametros_planillas END DESC,
                    CASE WHEN @SortField = 'PARAMETROS_ADD' AND @SortOrder = 'ASC' THEN p.parametros_add END ASC,
                    CASE WHEN @SortField = 'PARAMETROS_ADD' AND @SortOrder = 'DESC' THEN p.parametros_add END DESC,
                    p.transaccion ASC
              OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                return new CcPlanillaProcesosComplementariosLista
                {
                    total = connection.QueryFirstOrDefault<int>(SqlProcesosComplementariosTotal, parametros),
                    lista = connection.Query<CcPlanillaProcesosComplementariosData>(sql, parametros).ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearResultadoListaVacio())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener procesos complementarios.", result.Code.GetValueOrDefault(-1), CrearResultadoListaVacio());
        }

        /// <summary>
        /// Obtiene lista completa (sin paginación) de procesos complementarios de planillas (PRM_PROCESOS_ADD) aplicando filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CcPlanillaProcesosComplementariosData>> CC_PlanillaProcesosComplementarios_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de procesos complementarios son requeridos.", -2, new List<CcPlanillaProcesosComplementariosData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new DynamicParameters();
                parametros.Add("@filtro", string.IsNullOrWhiteSpace(filtros.filtro) ? null : filtros.filtro.Trim());

                const string sql = SqlProcesosComplementariosBase + @"
              ORDER BY p.transaccion,
                       p.ejecucion_tipo,
                       CASE
                           WHEN ISNUMERIC(LTRIM(RTRIM(p.ejecucion_orden))) = 1 THEN CAST(LTRIM(RTRIM(p.ejecucion_orden)) AS int)
                           ELSE 0
                       END;";

                return connection.Query<CcPlanillaProcesosComplementariosData>(sql, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CcPlanillaProcesosComplementariosData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener procesos complementarios.", result.Code.GetValueOrDefault(-1), new List<CcPlanillaProcesosComplementariosData>());
        }

        /// <summary>
        /// Inserta o actualiza un proceso complementario de planillas (PRM_PROCESOS_ADD).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CC_PlanillaProcesosComplementarios_Guardar(int CodEmpresa,string usuario, CcPlanillaProcesosComplementariosData data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Datos inválidos.", -2);
            }

            var transaccion = (data.transaccion ?? string.Empty).Trim();
            var ejecucionTipo = (data.ejecucion_tipo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(transaccion))
            {
                return DbHelper.ErrorResponse("Transacción es requerida.", -2);
            }

            if (data.isNew && string.IsNullOrWhiteSpace(ejecucionTipo))
            {
                return DbHelper.ErrorResponse(
                    "Tipo de ejecución es requerido.",
                    -2);
            }

            if (!data.isNew && data.proc_num <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El número de proceso es requerido.",
                    -2);
            }

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                CodEmpresa,
                connection =>
                {
                    var existe = connection.QueryFirstOrDefault<int>(
                        @"SELECT COUNT(1)
                  FROM prm_procesos_add
                  WHERE RTRIM(ISNULL(transaccion, '')) = @transaccion
                    AND proc_num = @proc_num
                    AND RTRIM(ISNULL(ejecucion_tipo, '')) = @ejecucion_tipo;",
                        new
                        {
                            transaccion,
                            proc_num = data.proc_num,
                            ejecucion_tipo = ejecucionTipo
                        });

                    if (data.isNew)
                    {
                        if (existe > 0)
                        {
                            return DbHelper.ErrorResponse(
                                "El registro ya existe.",
                                -2);
                        }

                        data.proc_num = connection.QueryFirstOrDefault<int>(
                            @"SELECT ISNULL(MAX(proc_num), 0) + 1
                      FROM prm_procesos_add
                      WHERE RTRIM(ISNULL(transaccion, '')) = @transaccion;",
                            new
                            {
                                transaccion
                            });

                        return CC_PlanillaProcesosComplementarios_Insertar(
                            connection,
                            CodEmpresa,
                            usuario,
                            data);
                    }

                    if (existe == 0)
                    {
                        return DbHelper.ErrorResponse(
                            "El registro no existe.",
                            -2);
                    }

                    return CC_PlanillaProcesosComplementarios_Actualizar(
                        connection,
                        CodEmpresa,
                        usuario,
                        data);
                });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al guardar proceso complementario.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un proceso complementario de planillas (PRM_PROCESOS_ADD).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto CC_PlanillaProcesosComplementarios_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CcPlanillaProcesosComplementariosData data)
        {
            connection.Execute(
                @"INSERT INTO prm_procesos_add
                (transaccion, proc_num, ejecucion_orden, ejecucion_tipo, procedimiento, descripcion, parametros_planillas, parametros_add, registro_usuario, registro_fecha)
              VALUES
                (@transaccion, @proc_num, @ejecucion_orden, @ejecucion_tipo, @procedimiento, @descripcion, @parametros_planillas, @parametros_add, @registro_usuario, dbo.MyGetdate());",
                new
                {
                    transaccion = (data.transaccion ?? string.Empty).Trim(),
                    proc_num = data.proc_num,
                    ejecucion_orden = FormatearOrden(data.ejecucion_orden),
                    ejecucion_tipo = (data.ejecucion_tipo ?? string.Empty).Trim(),
                    procedimiento = (data.procedimiento ?? string.Empty).Trim(),
                    descripcion = (data.descripcion ?? string.Empty).Trim(),
                    parametros_planillas = data.parametros_planillas ? 1 : 0,
                    parametros_add = (data.parametros_add ?? string.Empty).Trim(),
                    registro_usuario = usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Planilla Proc.Add.: Tra: {data.transaccion} Tipo: {data.ejecucion_tipo} Id: {data.proc_num}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza un proceso complementario de planillas (PRM_PROCESOS_ADD).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto CC_PlanillaProcesosComplementarios_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CcPlanillaProcesosComplementariosData data)
        {
            connection.Execute(
                @"UPDATE prm_procesos_add
                 SET ejecucion_orden = @ejecucion_orden,
                     procedimiento = @procedimiento,
                     descripcion = @descripcion,
                     parametros_planillas = @parametros_planillas,
                     parametros_add = @parametros_add,
                     actualiza_usuario = @actualiza_usuario,
                     actualiza_fecha = dbo.MyGetdate()
               WHERE transaccion = @transaccion
                 AND proc_num = @proc_num
                 AND ejecucion_tipo = @ejecucion_tipo;",
                new
                {
                    transaccion = (data.transaccion ?? string.Empty).Trim(),
                    proc_num = data.proc_num,
                    ejecucion_tipo = (data.ejecucion_tipo ?? string.Empty).Trim(),
                    ejecucion_orden = FormatearOrden(data.ejecucion_orden),
                    procedimiento = (data.procedimiento ?? string.Empty).Trim(),
                    descripcion = (data.descripcion ?? string.Empty).Trim(),
                    parametros_planillas = data.parametros_planillas ? 1 : 0,
                    parametros_add = (data.parametros_add ?? string.Empty).Trim(),
                    actualiza_usuario = usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Planilla Proc.Add.: Tra: {data.transaccion} Tipo: {data.ejecucion_tipo} Id: {data.proc_num}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un proceso complementario de planillas (PRM_PROCESOS_ADD) por llave (transaccion, proc_num, ejecucion_tipo).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="transaccion"></param>
        /// <param name="proc_num"></param>
        /// <param name="ejecucion_tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CC_PlanillaProcesosComplementarios_Eliminar(int CodEmpresa,string transaccion, int proc_num,string ejecucion_tipo,string usuario)
        {
            var transaccionNormalizada =
                (transaccion ?? string.Empty).Trim();

            var ejecucionTipoNormalizado =
                (ejecucion_tipo ?? string.Empty).Trim();

            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                @"DELETE FROM prm_procesos_add
          WHERE RTRIM(ISNULL(transaccion, '')) = @transaccion
            AND proc_num = @proc_num
            AND RTRIM(ISNULL(ejecucion_tipo, '')) = @ejecucion_tipo;",
                new
                {
                    transaccion = transaccionNormalizada,
                    proc_num,
                    ejecucion_tipo = ejecucionTipoNormalizado
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al eliminar proceso complementario.",
                    result.Code.GetValueOrDefault(-1));
            }

            if (result.Result <= 0)
            {
                return new ErrorDto
                {
                    Code = 1,
                    Description = "No se encontró el registro"
                };
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Planilla Proc.Add.: Tra: {transaccionNormalizada} " +
                $"Tipo: {ejecucionTipoNormalizado} Id: {proc_num}",
                "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Obtiene el dropdown de transacciones para procesos complementarios de planillas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_Transacciones_Obtener(int CodEmpresa)
        {
            const string query = @"SELECT CAST('01' AS varchar(2)) AS item, 'Cambia Fecha de Proceso' AS descripcion
                      UNION ALL SELECT '02','Genera deducciones'
                      UNION ALL SELECT '03','Carga deducciones'
                      UNION ALL SELECT '04','Desglosa deducciones'
                      UNION ALL SELECT '05','Aplica Ahorros'
                      UNION ALL SELECT '06','Inconsistencias de Ahorros'
                      UNION ALL SELECT '07','Devoluciones de Ahorros'
                      UNION ALL SELECT '08','Aplica Abonos'
                      UNION ALL SELECT '09','Reporte de Inconsistencias'
                      UNION ALL SELECT '10','Actualiza Intereses Moratorios'
                      UNION ALL SELECT '11','Actualiza Saldo del Mes'
                      ORDER BY item;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene la lista de tipos de ejecución (EJECUCION_TIPO) para PRM_PROCESOS_ADD.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT DISTINCT
                    ejecucion_tipo AS item,
                    ejecucion_tipo AS descripcion
                FROM prm_procesos_add
                WHERE ejecucion_tipo IS NOT NULL
                  AND LTRIM(RTRIM(ejecucion_tipo)) <> ''
                ORDER BY ejecucion_tipo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query);
        }

        private static CcPlanillaProcesosComplementariosLista CrearResultadoListaVacio()
        {
            return new CcPlanillaProcesosComplementariosLista
            {
                total = 0,
                lista = new List<CcPlanillaProcesosComplementariosData>()
            };
        }


        private static string NormalizarSortFieldProcesos(string? sortField)
        {
            return (sortField ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                CampoTransaccion => CampoTransaccion,
                CampoProceso => CampoProceso,
                CampoProcNum => CampoProcNum,
                CampoEjecucionTipo => CampoEjecucionTipo,
                CampoEjecucionOrden => CampoEjecucionOrden,
                CampoProcedimiento => CampoProcedimiento,
                CampoDescripcion => CampoDescripcion,
                CampoParametrosPlanillas => CampoParametrosPlanillas,
                CampoParametrosAdd => CampoParametrosAdd,
                _ => CampoTransaccion
            };
        }

        private static string FormatearOrden(string? orden)
        {
            return int.TryParse((orden ?? string.Empty).Trim(), out var ordenNum)
                ? ordenNum.ToString().PadLeft(3, '0')
                : 0.ToString().PadLeft(3, '0');
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
