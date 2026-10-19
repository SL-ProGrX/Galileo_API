using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosReportesBl
    {
        private readonly FrmCrConveniosReportesDb _db;

        public FrmCrConveniosReportesBl(IConfiguration config)
        {
            _db = new FrmCrConveniosReportesDb(config);
        }

        public ErrorDto<List<CrConveniosReportesCatalogoData>> CrConveniosReportes_Catalogo_Obtener(int codEmpresa)
            => _db.CrConveniosReportes_Catalogo_Obtener(codEmpresa);

        public ErrorDto<CrConveniosReportesCatalogoData> CrConveniosReportes_Catalogo_Describe(int codEmpresa, string codigo)
            => _db.CrConveniosReportes_Catalogo_Describe(codEmpresa, codigo);
    }
}
