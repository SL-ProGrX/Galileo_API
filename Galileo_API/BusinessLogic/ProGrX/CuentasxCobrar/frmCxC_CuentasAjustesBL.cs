using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAjustesBl
    {
        private readonly FrmCxCCuentasAjustesDb _db;

        public FrmCxCCuentasAjustesBl(IConfiguration config) => _db = new FrmCxCCuentasAjustesDb(config);

        public ErrorDto<CxCCuentasAjustesOperacionData> CxCCuentasAjustes_ConsultaOperacion_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCCuentasAjustes_ConsultaOperacion_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CxCCuentasAjustesCuotasData>> CxCCuentasAjustes_CuotasMora_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCCuentasAjustes_CuotasMora_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CxCCuentasAjustesCargosData>> CxCCuentasAjustes_Cargos_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCCuentasAjustes_Cargos_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto CxCCuentasAjustes_Fecha_Aplicar(int codEmpresa, CxCCuentasAjustesFechaRequest request)
        {
            return _db.CxCCuentasAjustes_Fecha_Aplicar(codEmpresa, request);
        }

        public ErrorDto CxCCuentasAjustes_CuotasMora_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCuotasData> lista)
        {
            return _db.CxCCuentasAjustes_CuotasMora_Eliminar(codEmpresa, operacionId, usuario, lista);
        }

        public ErrorDto CxCCuentasAjustes_Cargos_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCargosData> lista)
        {
            return _db.CxCCuentasAjustes_Cargos_Eliminar(codEmpresa, operacionId, usuario, lista);
        }
    }
}
