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
        /// Plantilla Rate Obtener
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PlantillaRate_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                cod_plantilla AS Item,
                descripcion   AS Descripcion
            FROM CntX_Plantilla_Rate
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_plantilla";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Plantilla Rate Detalle
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPlantillaRateDetalleDto>> PlantillaRate_Detalle(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            var response = new ErrorDto<List<CntxPlantillaRateDetalleDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                P.cod_cuenta,
                C.cod_Cuenta_Mask      AS cod_cuenta_mask,
                C.descripcion          AS descripcion,
                P.debitos,
                P.creditos,
                P.detalle
            FROM CntX_Plantilla_Rate_Detalle P
            INNER JOIN CntX_Cuentas C
                ON P.cod_contabilidad = C.cod_contabilidad
                AND P.cod_cuenta = C.cod_cuenta
            WHERE P.cod_contabilidad = @codContabilidad
              AND P.cod_plantilla = @codPlantilla
            ORDER BY P.num_linea";

                response.Result = cn.Query<CntxPlantillaRateDetalleDto>(
                    sql,
                    new { codContabilidad, codPlantilla }
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
