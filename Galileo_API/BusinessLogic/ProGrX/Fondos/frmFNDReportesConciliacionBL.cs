using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndReportesConciliacionBL
    {
        private readonly FrmFndReportesConciliacionDB _db;

        public FrmFndReportesConciliacionBL(IConfiguration config)
        {
            _db = new FrmFndReportesConciliacionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Operadoras_Obtener(int codEmpresa)
        {
            return _db.ReportesConciliacion_Operadoras_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Entidades_Obtener(int codEmpresa)
        {
            return _db.ReportesConciliacion_Entidades_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_PeriodosHistorico_Obtener(int codEmpresa)
        {
            return _db.ReportesConciliacion_PeriodosHistorico_Obtener(codEmpresa);
        }

        public ErrorDto<FndPerHistoricoDetalleModel?> ReportesConciliacion_PeriodoHistoricoDetalle_Obtener(int codEmpresa, string idPerHistorico)
        {
            return _db.ReportesConciliacion_PeriodoHistoricoDetalle_Obtener(codEmpresa, idPerHistorico);
        }
    }
}
