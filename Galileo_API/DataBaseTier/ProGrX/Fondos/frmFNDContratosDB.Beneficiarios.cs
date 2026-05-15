using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SqlContratoBeneficiarios = @"
                    SELECT
                        CedulaBn,
                        Nombre,
                        Porcentaje,
                        parentesco,
                        P.Descripcion AS parentesco_desc
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS B
                    LEFT JOIN dbo.sys_Parentescos P
                        ON P.cod_Parentesco = B.parentesco
                    WHERE B.Cedula = @Cedula
                      AND B.cod_contrato = @Contrato
                      AND B.cod_operadora = @Operadora
                      AND B.cod_plan = @Plan
                      AND P.activo = 1;";

        #region Beneficiarios

        /// <summary>
        /// Obtiene los beneficiarios asociados a un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <param name="cedula">Cédula del titular.</param>
        /// <returns>Listado de beneficiarios registrados.</returns>
        public ErrorDto<List<FndContratoBeneficiariosData>> Fnd_Contratos_Beneficiarios_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            return DbHelper.ExecuteListQuery<FndContratoBeneficiariosData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlContratoBeneficiarios,
                new
                {
                    Cedula = NormalizarTexto(cedula),
                    Contrato = pContrato,
                    Operadora = pOperadora,
                    Plan = NormalizarTexto(pPlan)
                });
        }

        #endregion
    }
}