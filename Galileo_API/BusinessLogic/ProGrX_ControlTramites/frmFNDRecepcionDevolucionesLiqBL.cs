using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionDevolucionesLiqBl
    {
        private readonly FrmFndRecepcionDevolucionesLiqDb _db;

        public FrmFndRecepcionDevolucionesLiqBl(IConfiguration config)
        {
            _db = new FrmFndRecepcionDevolucionesLiqDb(config);
        }

        public ErrorDto<FndRecepcionDevolucionesLiqInicializarData>
            FND_frmFNDRecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            return _db.FND_frmFNDRecepcionDevolucionesLiq_Inicializar(
                codEmpresa);
        }

        public ErrorDto<FndRecepcionDevolucionesLiqData?>
            FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta)
        {
            return _db.FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener(
                codEmpresa,
                numeroBoleta);
        }

        public ErrorDto<FndRecepcionDevolucionesLiqAplicarData>
            FND_frmFNDRecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesLiqAplicarRequest request)
        {
            return _db.FND_frmFNDRecepcionDevolucionesLiq_Aplicar(
                codEmpresa,
                request);
        }
    }
}
