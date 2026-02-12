using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAbonosBl
    {
        private readonly FrmCxCCuentasAbonosDb _db;

        public FrmCxCCuentasAbonosBl(IConfiguration config) => _db = new FrmCxCCuentasAbonosDb(config);

        public ErrorDto<CxCCuentasAbonosData> CxCCuentas_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
        {
            return _db.CxCCuentas_ConsultaOperacion_Obtener(codEmpresa, codCaja, operacionId);
        }

        public ErrorDto<List<CxCCuotasActivasData>> CxCCuentas_CuotasActivas_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCCuentas_CuotasActivas_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CxCOperacionesActivasData>> CxCCuentas_OperacionesActivas_Obtener(int codEmpresa)
        {
            return _db.CxCCuentas_OperacionesActivas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            return _db.CxCCuentas_TipoDoc_Obtener(codEmpresa, caja);
        }

        public ErrorDto<CxCCuentaCuotasInfoData> CxCCuentas_CuotasInfo_Obtener(int codEmpresa, int vOperacion, int vCuotas)
        {
            return _db.CxCCuentas_CuotasInfo_Obtener(codEmpresa, vOperacion, vCuotas);
        }

        public ErrorDto CxCCuentas_Abono_Registrar(int codEmpresa, CxCCuentasRegistrarAbonoRequest request)
        {
            return _db.CxCCuentas_Abono_Registrar(codEmpresa, request);
        }
    }
}
