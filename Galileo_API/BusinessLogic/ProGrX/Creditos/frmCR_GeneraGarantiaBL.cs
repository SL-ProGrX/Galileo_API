using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrGeneraGarantiaBl
    {
        private readonly FrmCrGeneraGarantiaDb _db;

        /// <summary>
        /// Inicializa la lógica de negocio para la emisión de garantías.
        /// </summary>
        /// <param name="config">Configuración de acceso a datos.</param>
        public FrmCrGeneraGarantiaBl(IConfiguration config)
        {
            _db = new FrmCrGeneraGarantiaDb(config);
        }

        /// <summary>
        /// Prepara los datos de un pagaré.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación y opciones de emisión.</param>
        /// <returns>Datos requeridos por el reporte del pagaré.</returns>
        public ErrorDto<CrGeneraGarantiaPagareDto> CR_GeneraGarantia_Pagare_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _db.CR_GeneraGarantia_Pagare_Preparar(codEmpresa, request);

        /// <summary>
        /// Prepara los datos de un contrato de crédito.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que solicita el documento.</param>
        /// <param name="request">Operación y opciones de emisión.</param>
        /// <returns>Datos requeridos por el reporte del contrato.</returns>
        public ErrorDto<CrGeneraGarantiaContratoDto> CR_GeneraGarantia_Contrato_Preparar(
            int codEmpresa,
            string usuario,
            CrGeneraGarantiaOperacionRequest request) =>
            _db.CR_GeneraGarantia_Contrato_Preparar(codEmpresa, usuario, request);

        /// <summary>
        /// Genera y envía el pagaré digital.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación y opciones de emisión.</param>
        /// <returns>Correo al que se envió el documento.</returns>
        public ErrorDto<CrGeneraGarantiaEmailDto> CR_GeneraGarantia_PagareEmail_Enviar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _db.CR_GeneraGarantia_PagareEmail_Enviar(codEmpresa, request);

        /// <summary>
        /// Obtiene las operaciones elegibles para letras de cambio.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Rango de operaciones.</param>
        /// <returns>Operaciones y valores calculados para el reporte.</returns>
        public ErrorDto<List<CrGeneraGarantiaLetraDto>> CR_GeneraGarantia_Letras_Obtener(
            int codEmpresa,
            CrGeneraGarantiaRangoRequest request) =>
            _db.CR_GeneraGarantia_Letras_Obtener(codEmpresa, request);

        /// <summary>
        /// Prepara los datos del pagaré preimpreso.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación y opciones de emisión.</param>
        /// <returns>Textos y valores requeridos por el reporte preimpreso.</returns>
        public ErrorDto<CrGeneraGarantiaPreImpresoDto> CR_GeneraGarantia_PreImpreso_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _db.CR_GeneraGarantia_PreImpreso_Preparar(codEmpresa, request);
    }
}
