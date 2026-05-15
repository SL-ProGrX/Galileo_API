using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {

        private const string SqlPlanesRetiros = @"
                    SELECT
                        cod_fnd_tabla_ret AS id,
                        desde,
                        hasta,
                        porcentaje,
                        aplicar_a AS aplicar,
                        registro_usuario,
                        registro_fecha,
                        actualiza_usuario,
                        actualiza_fecha
                    FROM dbo.fnd_tabla_retiros
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan
                    ORDER BY desde;";

        #region Retiros

        /// <summary>
        /// Obtiene la configuración de retiros asociada a un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codoperadora">Código de operadora.</param>
        /// <param name="codplan">Código del plan.</param>
        /// <returns>Listado de reglas de retiro configuradas para el plan.</returns>
        public ErrorDto<List<FndPlanRetiroDto>> Fnd_Planes_Retiros_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return DbHelper.ExecuteListQuery<FndPlanRetiroDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesRetiros,
                new
                {
                    codoperadora,
                    codplan = NormalizarTexto(codplan)
                });
        }

        #endregion

    }
}