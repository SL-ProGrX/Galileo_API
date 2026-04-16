using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlReportesBL
    {
        private readonly FrmCOControlReportesDB Db;

        public FrmCOControlReportesBL(IConfiguration config)
        {
            Db = new FrmCOControlReportesDB(config);
        }

        public ErrorDto<List<CoControlReporteItemDto>> CO_ControlReportes_Catalogo_Obtener(int CodEmpresa)
        {
            return Db.CO_ControlReportes_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<CoControlReportesFiltrosDto> CO_ControlReportes_Filtros_Obtener(int CodEmpresa)
        {
            return Db.CO_ControlReportes_Filtros_Obtener(CodEmpresa);
        }

        public ErrorDto CO_ControlReportes_Cubo_Procesar(int CodEmpresa, CoControlReportesCuboRequestDto data)
        {
            return Db.CO_ControlReportes_Cubo_Procesar(CodEmpresa, data);
        }
    }
}
