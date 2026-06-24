using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvReporteInventariosBL
    {
        private readonly FrmInvReporteInventariosDB _db;

        public FrmInvReporteInventariosBL(IConfiguration config)
        {
            _db = new FrmInvReporteInventariosDB(config);
        }

        public ErrorDto<List<LineasInvMCdto>> Obtener_Lineas(int CodEmpresa)
        {
            return _db.Obtener_Lineas(CodEmpresa);
        }

        public ErrorDto<List<BodegaReporteInvMCdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _db.Obtener_Bodegas(CodEmpresa);
        }

    }
}