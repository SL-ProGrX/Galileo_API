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
        /// Obtiene las ramas habilitadas del explorador para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Configuración de ramas visibles del árbol contable.</returns>
        public ErrorDto<CntxConfiguracionArbolDto> ConfiguracionArbol_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            var response = DbHelper.CreateOkResponse(new CntxConfiguracionArbolDto());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        CAST(ISNULL(expasientos, 0) AS bit) AS exp_asientos,
                        CAST(ISNULL(expcuentas, 0) AS bit) AS exp_cuentas,
                        CAST(ISNULL(expareas, 0) AS bit) AS exp_areas,
                        CAST(ISNULL(expplanfijo, 0) AS bit) AS exp_plan_fijo,
                        CAST(ISNULL(expplanrate, 0) AS bit) AS exp_plan_rate,
                        CAST(ISNULL(expdiferidos, 0) AS bit) AS exp_diferidos,
                        CAST(ISNULL(expmantenimiento, 0) AS bit) AS exp_mantenimiento
                    FROM CntX_Contabilidades
                    WHERE cod_contabilidad = @codContabilidad";

                response.Result = cn.QuerySingleOrDefault<CntxConfiguracionArbolDto>(
                    sql,
                    new { codContabilidad }) ?? new CntxConfiguracionArbolDto();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntxConfiguracionArbolDto>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtener contabilidades
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxContabilidadDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

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
                return DbHelper.CreateErrorResponse<List<CntxContabilidadDto>>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtener Cierres
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_contabilidad">Código de la contabilidad seleccionada.</param>
        /// <returns>Resultado de la operación solicitado por el explorador contable.</returns>
        public ErrorDto<List<CntxCierreDto>> ObtenerCierres(int codEmpresa, int cod_contabilidad)
        {
            var response = DbHelper.CreateOkResponse(new List<CntxCierreDto>());

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, codEmpresa);

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
                return DbHelper.CreateErrorResponse<List<CntxCierreDto>>(ex.Message);
            }

            return response;
        }
    }
}
