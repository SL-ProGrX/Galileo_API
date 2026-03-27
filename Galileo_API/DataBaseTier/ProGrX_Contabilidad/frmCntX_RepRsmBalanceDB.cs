using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRsmBalanceDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXRsmBalanceDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXRsmBalanceDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Unidades listar
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
                new { cod_contabilidad = codContabilidad }
            );
        }

        /// <summary>
        /// Centrode costos listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad, string unidad)
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
        /// Genera reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<bool> GenerarReporte(int codEmpresa, int codContabilidad, CntxRsmBalanceFiltroDto f)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                if (f.reporte == "2" || f.reporte == "3")
                {
                    cn.Execute(@"
                        EXEC spCntX_BalanceRsmAnterior
                        @usuario,
                        @anio,
                        @mes
                    ", new
                    {
                        usuario = f.usuario,
                        anio = DateTime.Now.Year,
                        mes = DateTime.Now.Month
                    });
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
    }
}