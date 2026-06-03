using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFIngresosConsultaBL
    {
        private readonly FrmAFIngresosConsultaDB _db;

        public FrmAFIngresosConsultaBL(IConfiguration config)
        {
            _db = new FrmAFIngresosConsultaDB(config);
        }

        public ErrorDto<IngresosConsultaLista> AF_Ingresos_Consulta(int CodEmpresa, IngresosConsultaFiltro filtro)
        {
            return _db.AF_Ingresos_Consulta(CodEmpresa, filtro);
        }
    }
}
