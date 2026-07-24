using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public sealed class FrmCcPolizaBeneficiariosBL
    {
        private readonly FrmCcPolizaBeneficiariosDB _db;

        public FrmCcPolizaBeneficiariosBL(IConfiguration config)
        {
            _db = new FrmCcPolizaBeneficiariosDB(config);
        }

        public ErrorDto<CcPolizaBeneficiariosCatalogosDto> CC_Poliza_Beneficiarios_Catalogos_Obtener(
            int codEmpresa) =>
            _db.CC_Poliza_Beneficiarios_Catalogos_Obtener(codEmpresa);

        public ErrorDto<List<CcPolizaBeneficiarioDto>> CC_Poliza_Beneficiarios_Obtener(
            int codEmpresa,
            string cedula,
            string codPoliza) =>
            _db.CC_Poliza_Beneficiarios_Obtener(codEmpresa, cedula, codPoliza);

        public ErrorDto<CcPolizaBeneficiariosPadronDto?> CC_Poliza_Beneficiarios_Padron_Obtener(
            string identificacion) =>
            _db.CC_Poliza_Beneficiarios_Padron_Obtener(identificacion);

        public ErrorDto CC_Poliza_Beneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CcPolizaBeneficiariosGuardarRequest request) =>
            _db.CC_Poliza_Beneficiarios_Guardar(codEmpresa, usuario, request);
    }
}
