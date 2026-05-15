using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {

        #region Cupones
        /// <summary>
        /// Obtiene los cupones asociados a un contrato de inversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <returns>Listado de cupones generados para el contrato.</returns>
        public ErrorDto<List<FndContratosCuponesData>> Fnd_Contratos_Cupones_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _mFNDFunciones.sbFnd_Contratos_Cupones(
                CodEmpresa,
                pOperadora,
                NormalizarTexto(pPlan),
                pContrato);
        }

        #endregion

    }
}