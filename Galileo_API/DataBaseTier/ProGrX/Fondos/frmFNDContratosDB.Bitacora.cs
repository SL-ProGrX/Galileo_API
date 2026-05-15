using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {

        #region Bitacora
        /// <summary>
        /// Obtiene la bitácora de movimientos asociados a un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pOperadora">Código de operadora.</param>
        /// <param name="pPlan">Código del plan.</param>
        /// <param name="pContrato">Número de contrato.</param>
        /// <returns>Listado de movimientos registrados para el contrato.</returns>
        public ErrorDto<List<FndContratoBitacoraData>> Fnd_Contratos_Bitacora_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _mFNDFunciones.sbFnd_Contratos_Bitacora(
                CodEmpresa,
                pOperadora,
                NormalizarTexto(pPlan),
                pContrato);
        }

        #endregion

    }
}