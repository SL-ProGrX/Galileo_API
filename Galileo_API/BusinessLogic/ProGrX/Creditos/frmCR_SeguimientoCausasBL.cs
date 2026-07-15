using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRSeguimientoCausasModels;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRSeguimientoCausasBL
    {
        private readonly FrmCRSeguimientoCausasDB _db;

        public FrmCRSeguimientoCausasBL(IConfiguration config)
        {
            _db = new FrmCRSeguimientoCausasDB(config);
        }

        public ErrorDto<List<CrSeguimientoCausasData>> CR_SeguimientoCausas_Obtener(int codEmpresa, CrSeguimientoCausasObtenerRequest request)
            => _db.CR_SeguimientoCausas_Obtener(codEmpresa, request);

        public ErrorDto<bool> CR_SeguimientoCausas_Actualizar(int codEmpresa, CrSeguimientoCausasActualizarRequest request)
             => _db.CR_SeguimientoCausas_Actualizar(codEmpresa, request);
    }
}
