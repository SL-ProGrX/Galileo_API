using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcDocCuentasBl
    {
        private readonly FrmCcDocCuentasDb _db;

        public FrmCcDocCuentasBl(IConfiguration config)
        {
            _db = new FrmCcDocCuentasDb(config);
        }

        public ErrorDto<CcDocCuentasCuentaData> CC_DocCuentas_Cuenta_Obtener(int CodEmpresa, string? cuenta)
            => _db.CC_DocCuentas_Cuenta_Obtener(CodEmpresa, cuenta);

        public ErrorDto<CcDocCuentasVerificarData> CC_DocCuentas_Documento_Verificar(int CodEmpresa, string? tipo)
            => _db.CC_DocCuentas_Documento_Verificar(CodEmpresa, tipo);

        public ErrorDto<CcDocCuentasResultadoData> CC_DocCuentas_Documento_Validar(
            int CodEmpresa,
            CcDocCuentasValidarRequest request)
            => _db.CC_DocCuentas_Documento_Validar(CodEmpresa, request);
    }
}
