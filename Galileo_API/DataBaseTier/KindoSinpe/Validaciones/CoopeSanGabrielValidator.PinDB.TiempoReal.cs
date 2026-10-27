using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        /// <summary>
        /// Emisión de Tiempo Real por PIN. Igual que ASECCSS para cuentas PIN: el DocumentoBase es doc_base.
        /// </summary>
        public ErrorDto fxTesEmisionSinpeTiempoReal(
            int CodEmpresa,
            int Nsolicitud,
            DateTime vfecha,
            string vUsuario,
            int doc_base,
            int contador) =>
            EjecutarEmisionPIN(CodEmpresa, Nsolicitud, vfecha, vUsuario, contador, doc_base.ToString());
    }
}
