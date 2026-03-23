using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepMovPeriodoDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXRepMovPeriodoDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXRepMovPeriodoDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Periodos Listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_PeriodosRepMov_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT 
                    id_cierre AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Cierres
                WHERE cod_contabilidad = @cod_contabilidad
                ORDER BY id_cierre DESC
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad }
            );
        }

        /// <summary>
        /// Unidades listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_UnidadesRepMov_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT 
                    RTRIM(cod_unidad) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = @cod_contabilidad
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad }
            );
        }

        /// <summary>
        /// Centros listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostosRepMov_Listar(int codEmpresa, int codContabilidad, string unidad)
        {
            var sql = @"
                SELECT 
                    RTRIM(cod_centro_costo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Centro_Costos
                WHERE cod_contabilidad = @cod_contabilidad
            ";

            if (!string.IsNullOrEmpty(unidad) && unidad != "C")
            {
                sql += @"
                AND cod_centro_costo IN (
                    SELECT cod_centro_costo
                    FROM CntX_Unidades_CC
                    WHERE cod_contabilidad = @cod_contabilidad
                    AND cod_unidad = @unidad
                )";
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_contabilidad = codContabilidad,
                    unidad
                }
            );
        }

        /// <summary>
        /// Areas listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Areas_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT 
                    cod_area AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Area_Definicion
                WHERE cod_contabilidad = @cod_contabilidad
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad }
            );
        }

        /// <summary>
        /// Genera reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<bool> GenerarReporte(int codEmpresa, int codContabilidad, CntxRepMovPeriodoFiltroDto f)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                //----------------------------------------
                // 1. LIMPIAR
                //----------------------------------------
                cn.Execute(@"
            DELETE CntX_Rep_Periodos_mov
            WHERE usuario = @usuario
        ", new { usuario = f.usuario });

                //----------------------------------------
                // 2. INSERT BASE
                //----------------------------------------
                cn.Execute(@"
            INSERT INTO CntX_Rep_Periodos_mov
            (
                cod_cuenta,
                usuario,
                cod_contabilidad,
                movimiento_01,
                movimiento_02,
                movimiento_03,
                movimiento_04,
                movimiento_05,
                movimiento_06,
                movimiento_07,
                movimiento_08,
                movimiento_09,
                movimiento_10,
                movimiento_11,
                movimiento_12
            )
            SELECT 
                cod_cuenta,
                @usuario,
                @cod_contabilidad,
                0,0,0,0,0,0,0,0,0,0,0,0
            FROM CntX_Cuentas
            WHERE cod_contabilidad = @cod_contabilidad
        ", new
                {
                    usuario = f.usuario,
                    cod_contabilidad = codContabilidad
                });

                //----------------------------------------
                // 3. OBTENER PERIODO
                //----------------------------------------
                var periodo = cn.QueryFirstOrDefault<dynamic>(@"
            SELECT inicio_mes, inicio_anio
            FROM CntX_Cierres
            WHERE id_cierre = @periodo
            AND cod_contabilidad = @cod_contabilidad
        ", new
                {
                    periodo = f.periodo,
                    cod_contabilidad = codContabilidad
                });

                if (periodo == null)
                    throw new Exception("No se encontró el periodo");

                int mes = periodo.inicio_mes;
                int anio = periodo.inicio_anio;

                //----------------------------------------
                // 4. LOOP 12 MESES 
                //----------------------------------------
                for (int i = 1; i <= 12; i++)
                {
                    //----------------------------------------
                    // CONSULTA BASE
                    //----------------------------------------
                    string sqlMovimiento = f.mostrar == "A"
                        ? @"SELECT 
                        cod_cuenta,
                        saldo_inicial + total_debitos + total_creditos AS movimiento
                   FROM vCntX_Mov_Cuentas_General
                   WHERE anio = @anio AND mes = @mes AND cod_contabilidad = @cod_contabilidad"
                        : @"SELECT 
                        cod_cuenta,
                        total_debitos + total_creditos AS movimiento
                   FROM vCntX_Mov_Cuentas_General
                   WHERE anio = @anio AND mes = @mes AND cod_contabilidad = @cod_contabilidad";

                    //----------------------------------------
                    // FILTROS DINÁMICOS
                    //----------------------------------------
                    if (!string.IsNullOrEmpty(f.unidad) && f.unidad != "C")
                        sqlMovimiento += " AND cod_unidad = @unidad";

                    if (!string.IsNullOrEmpty(f.centroCosto) && f.centroCosto != "T")
                        sqlMovimiento += " AND cod_centro_costo = @centro";

                    //----------------------------------------
                    // EJECUTAR QUERY
                    //----------------------------------------
                    var movimientos = cn.Query(sqlMovimiento, new
                    {
                        anio,
                        mes,
                        cod_contabilidad = codContabilidad,
                        unidad = f.unidad,
                        centro = f.centroCosto
                    });

                    //----------------------------------------
                    // SQL SEGURO (SIN DINÁMICO)
                    //----------------------------------------
                    string sqlUpdate = ObtenerSqlUpdate(i);

                    //----------------------------------------
                    // UPDATE
                    //----------------------------------------
                    using (var transaction = cn.BeginTransaction())
                    {
                        foreach (var m in movimientos)
                        {
                            cn.Execute(sqlUpdate, new
                            {
                                movimiento = m.movimiento,
                                cod_cuenta = m.cod_cuenta,
                                usuario = f.usuario,
                                cod_contabilidad = codContabilidad
                            }, transaction: transaction);
                        }
                        transaction.Commit();
                    }

                    //----------------------------------------
                    // SIGUIENTE MES
                    //----------------------------------------
                    if (mes == 12)
                    {
                        mes = 1;
                        anio++;
                    }
                    else
                    {
                        mes++;
                    }
                }

                //----------------------------------------
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private string ObtenerSqlUpdate(int i)
        {
            return i switch
            {
                1 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_01 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                2 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_02 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                3 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_03 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                4 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_04 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                5 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_05 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                6 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_06 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                7 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_07 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                8 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_08 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                9 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_09 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                10 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_10 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                11 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_11 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                12 => @"UPDATE CntX_Rep_Periodos_mov SET movimiento_12 = @movimiento WHERE cod_cuenta = @cod_cuenta AND usuario = @usuario AND cod_contabilidad = @cod_contabilidad",
                _ => throw new Exception("Mes inválido")
            };
        }
    }
}