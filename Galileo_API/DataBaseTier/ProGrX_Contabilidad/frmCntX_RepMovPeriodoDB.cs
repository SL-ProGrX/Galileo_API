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

                Limpiar(cn, f.usuario!);
                InsertBase(cn, f.usuario!, codContabilidad);

                if (!f.periodo.HasValue)
                    throw new ArgumentException("Periodo es requerido");

                var (mes, anio) = ObtenerPeriodo(cn, f.periodo.Value, codContabilidad);

                for (int i = 1; i <= 12; i++)
                {
                    string sqlMovimiento = ConstruirSqlMovimiento(f);
                    sqlMovimiento = AplicarFiltros(sqlMovimiento, f);

                    var movimientos = cn.Query(sqlMovimiento, new
                    {
                        anio,
                        mes,
                        cod_contabilidad = codContabilidad,
                        unidad = f.unidad,
                        centro = f.centroCosto
                    });

                    string sqlUpdate = ObtenerSqlUpdate(i);

                    EjecutarUpdates(cn, movimientos, sqlUpdate, f.usuario!, codContabilidad);

                    (mes, anio) = SiguienteMes(mes, anio);
                }

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private static void Limpiar(SqlConnection cn, string usuario)
        {
            cn.Execute(@"
        DELETE CntX_Rep_Periodos_mov
        WHERE usuario = @usuario
    ", new { usuario });
        }

        private (int mes, int anio) ObtenerPeriodo(SqlConnection cn, int periodo, int codContabilidad)
        {
            var result = cn.QueryFirstOrDefault<dynamic>(@"
        SELECT inicio_mes, inicio_anio
        FROM CntX_Cierres
        WHERE id_cierre = @periodo
        AND cod_contabilidad = @cod_contabilidad
    ", new { periodo, cod_contabilidad = codContabilidad });

            if (result == null)
                throw new ArgumentException("No se encontró el periodo");

            return (result.inicio_mes, result.inicio_anio);
        }

        private void InsertBase(SqlConnection cn, string usuario, int codContabilidad)
        {
            cn.Execute(@"
        INSERT INTO CntX_Rep_Periodos_mov
        (
            cod_cuenta, usuario, cod_contabilidad,
            movimiento_01, movimiento_02, movimiento_03,
            movimiento_04, movimiento_05, movimiento_06,
            movimiento_07, movimiento_08, movimiento_09,
            movimiento_10, movimiento_11, movimiento_12
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
                usuario,
                cod_contabilidad = codContabilidad
            });
        }

        private static string ConstruirSqlMovimiento(CntxRepMovPeriodoFiltroDto f)
        {
            return f.mostrar == "A"
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
        }

        private static string AplicarFiltros(string sql, CntxRepMovPeriodoFiltroDto f)
        {
            if (!string.IsNullOrEmpty(f.unidad) && f.unidad != "C")
                sql += " AND cod_unidad = @unidad";

            if (!string.IsNullOrEmpty(f.centroCosto) && f.centroCosto != "T")
                sql += " AND cod_centro_costo = @centro";

            return sql;
        }

        private void EjecutarUpdates(SqlConnection cn, IEnumerable<dynamic> movimientos, string sqlUpdate, string usuario, int codContabilidad)
        {
            using var transaction = cn.BeginTransaction();

            foreach (var m in movimientos)
            {
                cn.Execute(sqlUpdate, new
                {
                    movimiento = m.movimiento,
                    cod_cuenta = m.cod_cuenta,
                    usuario,
                    cod_contabilidad = codContabilidad
                }, transaction: transaction);
            }

            transaction.Commit();
        }

        private static (int mes, int anio) SiguienteMes(int mes, int anio)
        {
            if (mes == 12)
                return (1, anio + 1);

            return (mes + 1, anio);
        }

        private static string ObtenerSqlUpdate(int i)
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
                _ => throw new ArgumentOutOfRangeException(nameof(i), "Mes inválido")
            };
        }
    }
}