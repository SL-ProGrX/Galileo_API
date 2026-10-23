using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Tarjeta de Crédito

        /// <summary>
        /// Obtiene las tarjetas de crédito y sus saldos para la persona consultada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Tarjetas de crédito asociadas a la persona.</returns>
        public ErrorDto<List<CrConsultaTarjetaCreditoData>> CR_ConsultaCrd_TarjetaCredito_Obtener(int codEmpresa, string cedula, string usuario)
        {
            var result = EjecutarStoredProcedureList<CrConsultaTarjetaCreditoData>(
                codEmpresa,
                "spCajasConsultaCxCTarjetaCredito",
                new { Cedula = cedula, Usuario = usuario });
            return result;
        }

        #endregion
    }
}
