using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepBalanceComprobacionDb
    {
        private readonly PortalDB _portalDB;
        private readonly MCntXPreliminaresDb _mCntXPreliminaresDb;

        public FrmCntXRepBalanceComprobacionDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCntXPreliminaresDb = new MCntXPreliminaresDb(config);
        }

        /// <summary>
        /// Genera los movimientos temporales requeridos por el balance preliminar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <param name="usuario"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<bool> CntX_Preliminar_Montar(
            int codEmpresa,
            int codContabilidad,
            int anio,
            int mes,
            string usuario,
            string unidad = "0x0")
        {
            return _mCntXPreliminaresDb.sbCntX_Preliminar_Montar(
                codEmpresa,
                codContabilidad,
                anio,
                mes,
                "A",
                usuario,
                unidad);
        }

        /// <summary>
        /// Lista las unidades 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                            SELECT
                                RTRIM(cod_unidad) AS item,
                                RTRIM(descripcion) AS descripcion,
                                Nivel AS nivel,
                                unidad_omision,
                                reporta_renta,
                                activa,
                                RTRIM(Cta_Renta) AS cta_renta,
                                RTRIM(Cta_Renta_Gasto) AS cta_renta_gasto
                            FROM CntX_Unidades
                            WHERE COD_CONTABILIDAD = 2
                            ORDER BY cod_unidad;
                        ";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql, new { codEmpresa })
                    .ToList();

                response.Code = 0;
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
