using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public sealed class FrmCrExcedenteDatosDb
    {
        private const string MensajeCedulaRequerida =
            "Debe indicar la c&eacute;dula.";

        private const string MensajeInformacionNoEncontrada =
            "No se encontr&oacute; informaci&oacute;n de excedentes para la persona indicada.";

        private readonly PortalDB _portalDb;

        public FrmCrExcedenteDatosDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el desglose del credito disponible sobre excedentes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<MCredito.CrExcedenteDisponibleData>
            Cr_ExcedenteDatos_Obtener(
                int codEmpresa,
                string cedula)
        {
            cedula = (cedula ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCedulaRequerida,
                    -2,
                    new MCredito.CrExcedenteDisponibleData());
            }

            MCredito.CrExcedenteDisponibleData? resultado =
                MCredito.fxExcedenteDisponible(
                    _portalDb,
                    codEmpresa,
                    cedula);

            if (resultado is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeInformacionNoEncontrada,
                    -2,
                    new MCredito.CrExcedenteDisponibleData());
            }

            return DbHelper.CreateOkResponse(
                resultado);
        }
    }
}