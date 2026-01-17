using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosCtPBl
    {
        private readonly FrmCajasCrdAbonosCtPDb _db;

        public FrmCajasCrdAbonosCtPBl(IConfiguration config) => _db = new FrmCajasCrdAbonosCtPDb(config);

        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _db.CajasCrdAbonosCtP_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }
    }
}
