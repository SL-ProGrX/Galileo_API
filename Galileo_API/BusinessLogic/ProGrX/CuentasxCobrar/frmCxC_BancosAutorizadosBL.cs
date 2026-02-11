using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCBancosAutorizadosBL
    {
        private readonly FrmCxCBancosAutorizadosDB _db;

        public FrmCxCBancosAutorizadosBL(IConfiguration config)
        {
            _db = new FrmCxCBancosAutorizadosDB(config);
        }

        public ErrorDto<bool> CxcBancosAutorizados_InsertarFaltantes(int codEmpresa, CxcBancoAutorizadoInsertParams param)
            => _db.CxcBancosAutorizados_InsertarFaltantes(codEmpresa, param);

        public ErrorDto<List<CxcBancoAutorizadoResult>> CxcBancosAutorizados_Lista(int codEmpresa)
            => _db.CxcBancosAutorizados_Lista(codEmpresa);

        public ErrorDto<bool> CxcBancosAutorizados_UpdateCheques(int codEmpresa, CxcBancoAutorizadoUpdateChequesParams param)
            => _db.CxcBancosAutorizados_UpdateCheques(codEmpresa, param);

        public ErrorDto<bool> CxcBancosAutorizados_UpdateTransferencias(int codEmpresa, CxcBancoAutorizadoUpdateTransferenciasParams param)
            => _db.CxcBancosAutorizados_UpdateTransferencias(codEmpresa, param);
    }
}
