using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Obtiene las plantillas de asientos fijos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Lista de plantillas fijas disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PlantillasFijas_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        cod_plantilla AS Item,
                        descripcion AS Descripcion
                    FROM CntX_Plantilla_Asientos
                    WHERE cod_contabilidad = @codContabilidad
                    ORDER BY cod_plantilla";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtiene el detalle de una plantilla de asientos fijos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="codPlantilla">Código de la plantilla seleccionada.</param>
        /// <returns>Líneas contables configuradas en la plantilla fija.</returns>
        public ErrorDto<List<CntxPlantillaFijaDetalleDto>> PlantillaFija_Detalle(
            int codEmpresa,
            int codContabilidad,
            int codPlantilla)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxPlantillaFijaDetalleDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        P.cod_cuenta,
                        C.cod_cuenta_mask,
                        C.descripcion,
                        P.debitos,
                        P.creditos,
                        CAST('' AS varchar(1)) AS detalle
                    FROM CntX_Plantilla_Detalle P
                    INNER JOIN CntX_Cuentas C
                        ON P.cod_contabilidad = C.cod_contabilidad
                        AND P.cod_cuenta = C.cod_cuenta
                    WHERE P.cod_contabilidad = @codContabilidad
                      AND P.cod_plantilla = @codPlantilla
                    ORDER BY P.num_linea";

                response.Result = cn.Query<CntxPlantillaFijaDetalleDto>(
                    sql,
                    new { codContabilidad, codPlantilla }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CntxPlantillaFijaDetalleDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Plantilla Rate Obtener
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PlantillaRate_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

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
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

            return response;
        }


        /// <summary>
        /// Plantilla Rate Detalle
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="codPlantilla">Código de la plantilla seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxPlantillaRateDetalleDto>> PlantillaRate_Detalle(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxPlantillaRateDetalleDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

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
                return DbHelper.CreateErrorResponse<List<CntxPlantillaRateDetalleDto>>(ex.Message);
            }

            return response;
        }
    }
}
