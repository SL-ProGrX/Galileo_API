using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {

        private const string SqlPlanesDestinosAhorro = @"
                    SELECT
                        ID_DESTINO,
                        descripcion,
                        activo
                    FROM dbo.FND_PLANES_DESTINOS_AHORRO
                    WHERE cod_plan = @CodPlan;";

        private const string SqlPlanesDestinosAsociados = @"
                    SELECT
                        D.cod_destino,
                        D.descripcion,
                        CASE WHEN A.cod_plan IS NULL THEN 0 ELSE 1 END AS asignado
                    FROM dbo.fnd_destinos D
                    LEFT JOIN dbo.fnd_planes_destinos A
                        ON D.cod_destino = A.cod_destino
                       AND A.cod_operadora = @CodOperadora
                       AND A.cod_plan = @CodPlan
                    WHERE D.activo = 1
                    ORDER BY D.descripcion;";

        #region Destinos

        /// <summary>
        /// Obtiene los destinos de ahorro configurados para un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodPlan">Código del plan.</param>
        /// <returns>Listado de destinos de ahorro asociados al plan.</returns>
        public ErrorDto<List<FndPlanesDestinoAhorroDto>> Fnd_Planes_DestinosAhorro_Obtener(int CodEmpresa, string CodPlan)
        {
            return DbHelper.ExecuteListQuery<FndPlanesDestinoAhorroDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesDestinosAhorro,
                new
                {
                    CodPlan = NormalizarTexto(CodPlan)
                });
        }

        /// <summary>
        /// Obtiene los destinos disponibles y asociados para un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codoperadora">Código de operadora.</param>
        /// <param name="codplan">Código del plan.</param>
        /// <returns>Listado de destinos con indicador de asignación.</returns>
        public ErrorDto<List<FndDestinoAsociadoDto>> Fnd_Planes_DestinosAsociaos_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return DbHelper.ExecuteListQuery<FndDestinoAsociadoDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesDestinosAsociados,
                new
                {
                    CodOperadora = codoperadora,
                    CodPlan = NormalizarTexto(codplan)
                });
        }

        #endregion

    }
}