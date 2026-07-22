using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaReportesBL
    {
        private readonly FrmCrApaReportesDB _db;

        public FrmCrApaReportesBL(IConfiguration config)
        {
            _db = new FrmCrApaReportesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Reportes_Acreedores_Dropdown_Obtener(int codEmpresa, string ordenarPor)
        {
            return _db.CR_APA_Reportes_Acreedores_Dropdown_Obtener(codEmpresa, ordenarPor);
        }

        public ErrorDto<List<CrApaReportesOperacion>> CR_APA_Reportes_Operaciones_Obtener(int codEmpresa, string codAcreedor)
        {
            return _db.CR_APA_Reportes_Operaciones_Obtener(codEmpresa, codAcreedor);
        }

        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Reportes_Acreedor_Obtener(int codEmpresa, string codAcreedor)
        {
            return _db.CR_APA_Reportes_Acreedor_Obtener(codEmpresa, codAcreedor);
        }

        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Reportes_Operacion_Obtener(int codEmpresa, string codAcreedor, string operacion)
        {
            return _db.CR_APA_Reportes_Operacion_Obtener(codEmpresa, codAcreedor, operacion);
        }

        public ErrorDto<List<CrApaReportesSaldoCorte>> CR_APA_Reportes_SaldosCorte_Obtener(int codEmpresa, DateTime fechaCorte)
        {
            return _db.CR_APA_Reportes_SaldosCorte_Obtener(codEmpresa, fechaCorte);
        }

        public ErrorDto<int> CR_APA_Reportes_AuxiliarCorte_Existe(int codEmpresa, DateTime fechaCorte)
        {
            return _db.CR_APA_Reportes_AuxiliarCorte_Existe(codEmpresa, fechaCorte);
        }

        public ErrorDto CR_APA_Reportes_AuxiliarCorte_Aplicar(int codEmpresa, DateTime fechaCorte)
        {
            return _db.CR_APA_Reportes_AuxiliarCorte_Aplicar(codEmpresa, fechaCorte);
        }
    }
}
