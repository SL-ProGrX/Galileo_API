using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_CxC;

namespace Galileo_API.BusinessLogic.ProGrX_CxC
{
    public class FrmCxCReportesBl
    {
        private readonly FrmCxCReportesDb _db;

        public FrmCxCReportesBl(IConfiguration config)
        {
            _db = new FrmCxCReportesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Clientes_Listar(int codEmpresa)
        {
            return _db.CxC_Clientes_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Pagadores_Listar(int codEmpresa)
        {
            return _db.CxC_Pagadores_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Conceptos_Listar(int codEmpresa)
        {
            return _db.CxC_Conceptos_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cargos_Listar(int codEmpresa)
        {
            return _db.CxC_Cargos_Listar(codEmpresa);
        }
    }
}