using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasMonitoreoBL
    {
        private readonly FrmCxCFacturasMonitoreoDB _db;

        public FrmCxCFacturasMonitoreoBL(IConfiguration config)
        {
            _db = new FrmCxCFacturasMonitoreoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoPersonas_Obtener(
            int codEmpresa,
            string ordenarPor,
            bool esPagador)
        {
            return _db.CxCFacturasMonitoreoPersonas_Obtener(codEmpresa, ordenarPor, esPagador);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoConceptos_Obtener(int codEmpresa)
        {
            return _db.CxCFacturasMonitoreoConceptos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoContratos_Obtener(int codEmpresa)
        {
            return _db.CxCFacturasMonitoreoContratos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoEstados_Obtener(int codEmpresa)
        {
            return _db.CxCFacturasMonitoreoEstados_Obtener(codEmpresa);
        }
    }
}
