using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier
{
    public class FrmCprPCPeriodosDB
    {
        private readonly PortalDB _portalDB;

        // Literales / estados como constantes (Sonar S1192)
        private const string EstadoPendiente = "P";
        private const string EstadoAprobado = "A";

        public FrmCprPCPeriodosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<List<CatalogosLista>> CprPeriodosContabilidades_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT 
                    cod_contabilidad AS item,
                    Nombre AS descripcion
                FROM CNTX_Contabilidades
                ORDER BY cod_contabilidad;";

            return DbHelper.ExecuteListQuery<CatalogosLista>(_portalDB, CodEmpresa, sql);
        }

        public ErrorDto<List<CatalogosLista>> CprPeriodosModelos_Obtener(int CodEmpresa, string usuario, int cod_contabilidad)
        {
            const string sql = @"
                SELECT 
                    P.cod_modelo AS item,
                    P.DESCRIPCION AS descripcion
                FROM PRES_MODELOS P
                INNER JOIN PRES_MODELOS_USUARIOS Pmu
                    ON P.cod_Contabilidad = Pmu.cod_contabilidad
                    AND P.cod_Modelo = Pmu.cod_Modelo
                    AND Pmu.Usuario = @usuario
                INNER JOIN CNTX_CIERRES Cc
                    ON P.cod_Contabilidad = Cc.cod_Contabilidad
                    AND P.ID_CIERRE = Cc.ID_CIERRE
                WHERE P.COD_CONTABILIDAD = @cod_contabilidad
                ORDER BY Cc.Inicio_Anio DESC;";

            return DbHelper.ExecuteListQuery<CatalogosLista>(
                _portalDB,
                CodEmpresa,
                sql,
                new { usuario, cod_contabilidad }
            );
        }

        public ErrorDto<CprPlanPeriodosDto> CprPeriodosPlan_Obtener(int CodEmpresa, int id_periodo)
        {
            const string sql = @"SELECT * FROM CPR_PLAN_PERIODOS WHERE ID_PERIODO = @id_periodo;";

            var r = DbHelper.ExecuteSingleQuery<CprPlanPeriodosDto>(
                _portalDB,
                CodEmpresa,
                sql,
                defaultValue: null,
                parameters: new { id_periodo }
            );

            return r.Code == 0
                ? new ErrorDto<CprPlanPeriodosDto> { Code = 0, Description = "Ok", Result = r.Result }
                : new ErrorDto<CprPlanPeriodosDto> { Code = -1, Description = r.Description, Result = null };
        }

        public ErrorDto<CprPeriodosPlanLista> CprPeriodosPlanLista_Obtener(int CodEmpresa, string filtros)
        {
            var response = new ErrorDto<CprPeriodosPlanLista>
            {
                Code = 0,
                Result = new CprPeriodosPlanLista { total = 0, lista = new List<CprPlanPeriodosDto>() }
            };

            try
            {
                var filtro = JsonConvert.DeserializeObject<CprPeriodosPlanFiltros>(filtros) ?? new CprPeriodosPlanFiltros();
                var p = new DynamicParameters();

                var where = BuildPlanPeriodosWhere(filtro, p);
                var paginaSql = BuildPaginacionSql(filtro, p);

                var countSql = $@"SELECT COUNT(*) FROM CPR_PLAN_PERIODOS {where};";

                var dataSql = $@"
                    SELECT ID_PERIODO, COD_CONTABILIDAD, INICIO, CORTE
                    FROM CPR_PLAN_PERIODOS
                    {where}
                    ORDER BY ID_PERIODO
                    {paginaSql};";

                var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, countSql, 0, p);
                if (totalResp.Code != 0)
                {
                    response.Code = -1;
                    response.Description = totalResp.Description;
                    response.Result = null;
                    return response;
                }

                response.Result.total = totalResp.Result;

                var listResp = DbHelper.ExecuteListQuery<CprPlanPeriodosDto>(_portalDB, CodEmpresa, dataSql, p);
                if (listResp.Code != 0)
                {
                    response.Code = -1;
                    response.Description = listResp.Description;
                    response.Result = null;
                    return response;
                }

                response.Result.lista = listResp.Result ?? new List<CprPlanPeriodosDto>();
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
                return response;
            }
        }

        public ErrorDto<CprPlanPeriodosDto> CprPeriodoPlan_Scroll(int CodEmpresa, int scroll, int? id_periodo)
        {
            try
            {
                // comportamiento original: si id_periodo es null, el WHERE compara con NULL y devuelve null.
                const string sqlNext = @"SELECT TOP 1 * FROM CPR_PLAN_PERIODOS WHERE ID_PERIODO > @id ORDER BY ID_PERIODO ASC;";
                const string sqlPrev = @"SELECT TOP 1 * FROM CPR_PLAN_PERIODOS WHERE ID_PERIODO < @id ORDER BY ID_PERIODO DESC;";

                var sql = scroll == 1 ? sqlNext : sqlPrev;

                var r = DbHelper.ExecuteSingleQuery<CprPlanPeriodosDto>(
                    _portalDB,
                    CodEmpresa,
                    sql,
                    defaultValue: null,
                    parameters: new { id = id_periodo }
                );

                return r.Code == 0
                    ? new ErrorDto<CprPlanPeriodosDto> { Code = 0, Description = "Ok", Result = r.Result }
                    : new ErrorDto<CprPlanPeriodosDto> { Code = -1, Description = r.Description, Result = null };
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprPlanPeriodosDto> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        public ErrorDto CprPeriodoPlan_Guardar(int CodEmpresa, CprPlanPeriodosDto periodo)
        {
            try
            {
                if (periodo == null) return DbHelper.ErrorResponse("Periodo inválido.", -1);

                return periodo.id_periodo == 0
                    ? CprPeriodoPlan_Insertar(CodEmpresa, periodo)
                    : CprPeriodoPlan_Actualizar(CodEmpresa, periodo);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private ErrorDto CprPeriodoPlan_Insertar(int CodEmpresa, CprPlanPeriodosDto periodo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    using var tx = conn.BeginTransaction();

                    // Siguiente ID (misma lógica original) dentro de tx para evitar carreras.
                    const string nextIdSql = @"SELECT ISNULL(MAX(ID_PERIODO), 0) + 1 FROM CPR_PLAN_PERIODOS;";
                    periodo.id_periodo = conn.ExecuteScalar<int>(nextIdSql, transaction: tx);

                    const string insSql = @"
                        INSERT INTO CPR_PLAN_PERIODOS
                        (
                            ID_PERIODO,
                            COD_CONTABILIDAD,
                            INICIO,
                            CORTE,
                            ESTADO,
                            NOTAS,
                            REGISTRO_FECHA,
                            REGISTRO_USUARIO
                        )
                        VALUES
                        (
                            @id_periodo,
                            @cod_contabilidad,
                            @inicio,
                            @corte,
                            @estado,
                            @notas,
                            GETDATE(),
                            @registro_usuario
                        );";

                    conn.Execute(insSql, new
                    {
                        id_periodo = periodo.id_periodo,
                        periodo.cod_contabilidad,
                        periodo.inicio,
                        periodo.corte,
                        estado = EstadoPendiente,
                        periodo.notas,
                        periodo.registro_usuario
                    }, tx);

                    tx.Commit();
                    return new ErrorDto { Code = periodo.id_periodo, Description = "Periodo agregado satisfactoriamente!" };
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private ErrorDto CprPeriodoPlan_Actualizar(int CodEmpresa, CprPlanPeriodosDto periodo)
        {
            try
            {
                const string sql = @"
                    UPDATE CPR_PLAN_PERIODOS
                    SET
                        COD_CONTABILIDAD = @cod_contabilidad,
                        INICIO = @inicio,
                        CORTE = @corte,
                        ESTADO = @estado,
                        NOTAS = @notas,
                        MODIFICA_FECHA = GETDATE(),
                        MODIFICA_USUARIO = @modifica_usuario
                    WHERE ID_PERIODO = @id_periodo;";

                var r = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
                {
                    periodo.cod_contabilidad,
                    periodo.inicio,
                    periodo.corte,
                    periodo.estado,
                    periodo.notas,
                    periodo.modifica_usuario,
                    periodo.id_periodo
                });

                return r.Code == 0
                    ? new ErrorDto { Code = periodo.id_periodo, Description = "Periodo actualizado satisfactoriamente!" }
                    : DbHelper.ErrorResponse(r.Description ?? "Error actualizando periodo.", -1);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto CprPeriodoPlan_Eliminar(int CodEmpresa, int id_periodo)
        {
            // Mantiene la semántica original: NO elimina.
            return new ErrorDto { Code = 0, Description = "No se puede eliminar un Periodo del Sistema!" };
        }

        public ErrorDto CprPeriodoPlan_Aprobar(int CodEmpresa, int id_periodo, string usuario)
        {
            try
            {
                const string sql = @"
                    UPDATE CPR_PLAN_PERIODOS
                    SET
                        ESTADO = @estado,
                        ACTUALIZA_FECHA = GETDATE(),
                        ACTUALIZA_USUARIO = @usuario
                    WHERE ID_PERIODO = @id_periodo;";

                var r = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
                {
                    estado = EstadoAprobado,
                    usuario,
                    id_periodo
                });

                return r.Code == 0 ? DbHelper.OkResponse("Periodo aprobado correctamente") : DbHelper.ErrorResponse(r.Description ?? "Error aprobando periodo.", -1);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto<CprModeloDateDatos> CprPeriodoPlanMeses_Obtener(string modelo)
        {
            var response = new ErrorDto<CprModeloDateDatos> { Code = 0, Result = new CprModeloDateDatos() };

            try
            {
                var cpr = JsonConvert.DeserializeObject<CprModeloFiltro>(modelo) ?? new CprModeloFiltro();

                const string sql = @"
                    SELECT TOP 1
                        Cc.INICIO_MES AS inicio_mes,
                        Cc.CORTE_MES AS corte_mes
                    FROM PRES_MODELOS P
                    INNER JOIN PRES_MODELOS_USUARIOS Pmu
                        ON P.cod_Contabilidad = Pmu.cod_contabilidad
                        AND P.cod_Modelo = Pmu.cod_Modelo
                        AND Pmu.Usuario = @usuario
                    INNER JOIN CNTX_CIERRES Cc
                        ON P.cod_Contabilidad = Cc.cod_Contabilidad
                        AND P.ID_CIERRE = Cc.ID_CIERRE
                    WHERE P.COD_CONTABILIDAD = @cod_contabilidad
                      AND P.COD_MODELO = @cod_modelo
                    ORDER BY Cc.Inicio_Anio DESC;";

                var datosResp = DbHelper.ExecuteSingleQuery<CprModeloDatos>(
                    _portalDB,
                    cpr.codEmpresa,
                    sql,
                    defaultValue: null,
                    parameters: new { usuario = cpr.usuario, cod_contabilidad = cpr.cod_Contabilidad, cod_modelo = cpr.cod_modelo }
                );

                if (datosResp.Code != 0) return new ErrorDto<CprModeloDateDatos> { Code = -1, Description = datosResp.Description, Result = null };
                if (datosResp.Result == null) return new ErrorDto<CprModeloDateDatos> { Code = -1, Description = "No se encontraron datos del modelo.", Result = null };

                var datos = datosResp.Result;

                // Si inicio_mes / corte_mes no vienen (0), evitamos excepción
                if (datos.inicio_mes <= 0 || datos.inicio_mes > 12 || datos.corte_mes <= 0 || datos.corte_mes > 12)
                    return new ErrorDto<CprModeloDateDatos> { Code = -1, Description = "Meses de inicio/corte inválidos para el modelo.", Result = null };

                var annoActual = DateTime.Now.Year;

                var fechaInicio = new DateTime(annoActual, datos.inicio_mes, 1, 0, 0, 0, DateTimeKind.Local);
                var ultimoDiaMesFin = DateTime.DaysInMonth(annoActual, datos.corte_mes);
                var fechaFin = new DateTime(annoActual, datos.corte_mes, ultimoDiaMesFin, 0, 0, 0, DateTimeKind.Local);

                response.Result.inicio_mes = fechaInicio;
                response.Result.corte_mes = fechaFin;

                return response;
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprModeloDateDatos> { Code = -1, Description = ex.Message, Result = null };
            }
        }

        // Helper: usa DbHelper.WithConn y devuelve ErrorDto plano
        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? (r.Result ?? DbHelper.ErrorResponse("Error desconocido.", -1))
                : DbHelper.ErrorResponse(r.Description ?? "Error desconocido.", -1);
        }

        private static string BuildPlanPeriodosWhere(CprPeriodosPlanFiltros filtro, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtro.filtro)) return string.Empty;

            p.Add("@Q", $"%{filtro.filtro}%");
            return @"
                WHERE
                    CAST(ID_PERIODO AS NVARCHAR(50)) LIKE @Q
                    OR CAST(COD_CONTABILIDAD AS NVARCHAR(50)) LIKE @Q
                    OR CAST(INICIO AS NVARCHAR(50)) LIKE @Q
                    OR CAST(CORTE AS NVARCHAR(50)) LIKE @Q";
        }

        private static string BuildPaginacionSql(CprPeriodosPlanFiltros filtro, DynamicParameters p)
        {
            if (filtro.pagina == null || filtro.paginacion == null) return string.Empty;

            // OJO: en el original pagina parecía ser "offset" ya calculado.
            p.Add("@Off", filtro.pagina);
            p.Add("@Take", filtro.paginacion);

            return "OFFSET @Off ROWS FETCH NEXT @Take ROWS ONLY";
        }
    }
}