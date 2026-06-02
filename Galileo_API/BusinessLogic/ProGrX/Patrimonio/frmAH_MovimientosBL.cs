using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHMovimientosBL
    {
        private readonly FrmAHMovimientosDB _db;

        public FrmAHMovimientosBL(IConfiguration config)
        {
            _db = new FrmAHMovimientosDB(config);
        }

        public ErrorDto<MovimientosPatrimonioFiltrosDto?> AH_Movimientos_Filtros_Obtener(int codEmpresa)
            => _db.AH_Movimientos_Filtros_Obtener(codEmpresa);

        public ErrorDto<List<MovimientosPatrimonioDto>> AH_Movimientos_Consulta_Obtener(
            int codEmpresa,
            MovimientosPatrimonioConsultaRequest request)
            => _db.AH_Movimientos_Consulta_Obtener(codEmpresa, request);
    }
}
