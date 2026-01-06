using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndReportesGeneralesBl
    {
        private readonly FrmFndReportesGeneralesDb _db;

        public FrmFndReportesGeneralesBl(IConfiguration config)
        {
            _db = new FrmFndReportesGeneralesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            return _db.Fnd_ReportesGenerales_Catalogo_Obtener(CodEmpresa, Index);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Planes_Obtener(int CodEmpresa, int CodOperadora, string? CodPlan, string? Usuario)
        {
            return _db.Fnd_ReportesGenerales_Planes_Obtener(CodEmpresa, CodOperadora, CodPlan, Usuario);
        }

        public ErrorDto<DropDownListaGenericaModel> Fnd_ReportesGenerales_Plan_Scroll_Obtener(int codEmpresa, int CodOperadora, string? CodPlan, int scrollCode)
        {
            return _db.Fnd_ReportesGenerales_Plan_Scroll_Obtener(codEmpresa, CodOperadora, CodPlan, scrollCode);
        }

        public ErrorDto Fnd_ReportesGenerales_CuboAplicar(int CodEmpresa, FndReportesGeneralesCuboFiltros Filtros)
        {
            return _db.Fnd_ReportesGenerales_CuboAplicar(CodEmpresa, Filtros);
        }
    }
}
