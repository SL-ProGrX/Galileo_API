using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepEspecialesDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXRepEspecialesDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXRepEspecialesDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Busca peridos que listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Periodos_Listar(int codEmpresa, int codContabilidad)
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
                new
                {
                    cod_contabilidad = codContabilidad
                }
            );
        }

        /// <summary>
        /// Busca unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
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
                new
                {
                    cod_contabilidad = codContabilidad
                }
            );
        }

        /// <summary>
        /// Busca centros de costo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad, string unidad)
        {
            const string sql = @"
                SELECT 
                    RTRIM(cod_centro_costo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Centro_Costos
                WHERE cod_contabilidad = @cod_contabilidad
                AND ( cod_centro_costo IN (
                            SELECT cod_centro_costo
                            FROM CntX_Unidades_CC
                            WHERE cod_contabilidad = @cod_contabilidad))";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_contabilidad = codContabilidad,
                }
            );
        }


        /// <summary>
        /// Realiza acciones para generar el reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<bool> GenerarReporte(int codEmpresa,int codContabilidad,CntxRepEspecialFiltroDto f)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                string sql;

                //--------------------------------------
                // 1 eliminar información anterior
                //--------------------------------------

                if (f.reporte == "2.1" || f.reporte == "2.2")
                {
                    sql = @"DELETE CNTX_REP_PERIODOS_MOV_UNIDAD
                    WHERE usuario = @usuario";

                    cn.Execute(sql, new { usuario = f.usuario });
                }
                else
                {
                    sql = @"DELETE CntX_Rep_Periodos_mov
                    WHERE usuario = @usuario";

                    cn.Execute(sql, new { usuario = f.usuario });
                }


                //--------------------------------------
                // 2 lógica principal VB6
                //--------------------------------------

                if (f.reporte == "2.1" || f.reporte == "2.2")
                {
                    EjecutarRentabilidadEspecial(cn, codContabilidad, f);
                }
                else
                {
                    EjecutarMovimientoCatalogo(cn, codContabilidad, f);
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

        private void EjecutarMovimientoCatalogo(SqlConnection cn,int codContabilidad,CntxRepEspecialFiltroDto f)
        {
            var sql = @"
                    INSERT INTO CntX_Rep_Periodos_mov
                    (
                        cod_cuenta,
                        usuario,
                        cod_contabilidad,
                        movimiento_10,
                        movimiento_11,
                        movimiento_12,
                        movimiento_01,
                        movimiento_02,
                        movimiento_03,
                        movimiento_04,
                        movimiento_05,
                        movimiento_06,
                        movimiento_07,
                        movimiento_08,
                        movimiento_09
                    )
                    SELECT
                        cod_cuenta,
                        @usuario,
                        @cod_contabilidad,
                        0,0,0,0,0,0,0,0,0,0,0,0
                    FROM CntX_Cuentas
                    WHERE cod_contabilidad = @cod_contabilidad
                    ";

            cn.Execute(sql, new
            {
                usuario = f.usuario,
                cod_contabilidad = codContabilidad
            });
        }

        private void EjecutarRentabilidadEspecial(SqlConnection cn,int codContabilidad,CntxRepEspecialFiltroDto f)
        {
            var sql = @"
                        INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                        (
                            cod_unidad,
                            cod_centro_costo,
                            usuario,
                            cod_contabilidad,
                            movimiento_10,
                            movimiento_11,
                            movimiento_12,
                            movimiento_01,
                            movimiento_02,
                            movimiento_03,
                            movimiento_04,
                            movimiento_05,
                            movimiento_06,
                            movimiento_07,
                            movimiento_08,
                            movimiento_09
                        )
                        SELECT
                            cod_unidad,
                            '',
                            @usuario,
                            @cod_contabilidad,
                            0,0,0,0,0,0,0,0,0,0,0,0
                        FROM CntX_Unidades
                        WHERE cod_contabilidad = @cod_contabilidad
                        ";

                                cn.Execute(sql, new
                                {
                                    usuario = f.usuario,
                                    cod_contabilidad = codContabilidad
                                });
                            }
                        }
}