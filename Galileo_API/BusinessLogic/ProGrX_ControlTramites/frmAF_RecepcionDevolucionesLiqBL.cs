using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesLiqBl
    {
        private readonly FrmAfRecepcionDevolucionesLiqDb _db;

        public FrmAfRecepcionDevolucionesLiqBl(IConfiguration config)
        {
            _db = new FrmAfRecepcionDevolucionesLiqDb(config);
        }

        public ErrorDto<AfRecepcionDevolucionesLiqInicializarData>
            AF_frmAF_RecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            return _db.AF_frmAF_RecepcionDevolucionesLiq_Inicializar(
                codEmpresa);
        }

        public ErrorDto<AfRecepcionDevolucionesLiqData?>
            AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                int numeroBoleta)
        {
            return _db.AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener(
                codEmpresa,
                numeroBoleta);
        }

        public ErrorDto<AfRecepcionDevolucionesLiqAplicarData>
            AF_frmAF_RecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesLiqAplicarRequest request)
        {
            return _db.AF_frmAF_RecepcionDevolucionesLiq_Aplicar(
                codEmpresa,
                request);
        }
    }
}