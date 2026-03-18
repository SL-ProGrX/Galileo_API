using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXCuentaForaneaInicializaBl
    {
        private readonly FrmCntXCuentaForaneaInicializaDb _db;

        public FrmCntXCuentaForaneaInicializaBl(IConfiguration config) =>
            _db = new FrmCntXCuentaForaneaInicializaDb(config);

        public ErrorDto<string?> CntXDivisaLocal_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDivisaLocal_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXCuentaForaneaData>> CntXCuentaForaneas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            return _db.CntXCuentaForaneas_Obtener(codEmpresa, codConta, codDivisa);
        }

        public ErrorDto<CntXCuentaMovSaldoData?> CntXCuentaMovSaldo_Obtener(int codEmpresa, int codConta, string codCuenta, int anio, int mes)
        {
            return _db.CntXCuentaMovSaldo_Obtener(codEmpresa, codConta, codCuenta, anio, mes);
        }

        public ErrorDto CntXCuentaForanea_Inicializar(int codEmpresa, CntXCuentaForaneaInicializaRequest request)
        {
            return _db.CntXCuentaForanea_Inicializar(codEmpresa, request);
        }
    }
}
