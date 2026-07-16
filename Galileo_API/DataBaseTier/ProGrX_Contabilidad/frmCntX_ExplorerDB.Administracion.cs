using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Obtener contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            var response = new ErrorDto<List<CntxContabilidadDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                        SELECT 
                            cod_contabilidad AS codigo,
                            nombre,
                            tel_central,
                            tel_fax,
                            contacto
                        FROM CntX_Contabilidades
                        ORDER BY cod_contabilidad";

                response.Result = cn.Query<CntxContabilidadDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener Cierres
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxCierreDto>> ObtenerCierres(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<CntxCierreDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT
                        INICIO_ANIO      AS in_anio,
                        INICIO_MES       AS in_mes,
                        CORTE_ANIO       AS co_anio,
                        CORTE_MES        AS co_mes,
                        DESCRIPCION      AS descripcion,
                        CUENTA_GANPER    AS gan_per,
                        CUENTA_UTILIDAD  AS exc_uti,
                        CUENTA_IMPRENTA  AS renta_cta,
                        IMPUESTO_RENTA   AS renta,
                        CASE 
                            WHEN ACTIVO = 1 THEN 'Si'
                            ELSE 'No'
                        END AS vigente
                    FROM CNTX_CIERRES
                    WHERE COD_CONTABILIDAD = @cod_contabilidad
                    ORDER BY INICIO_ANIO DESC, INICIO_MES DESC";

                response.Result = cn.Query<CntxCierreDto>(
                    sql,
                    new { cod_contabilidad }
                ).ToList();
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
