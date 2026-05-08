using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivHonorariosDetalleBl
    {
        private readonly FrmVivHonorariosDetalleDb _db;

        public FrmVivHonorariosDetalleBl(IConfiguration config)
            => _db = new FrmVivHonorariosDetalleDb(config);

        public ErrorDto<VivHonorariosDetalleOperacionData?> VivHonorariosDetalle_ObtenerOperacion(
            int codEmpresa, int operacion, int idContacto)
        {
            return _db.VivHonorariosDetalle_ObtenerOperacion(codEmpresa, operacion, idContacto);
        }

        public ErrorDto<List<VivHonorariosDetalleLineaData>> VivHonorariosDetalle_ObtenerLineas(int codEmpresa)
        {
            return _db.VivHonorariosDetalle_ObtenerLineas(codEmpresa);
        }

        public ErrorDto VivHonorariosDetalle_Guardar(
            int codEmpresa, string usuario, VivHonorariosDetalleGuardarRequest request)
        {
            return _db.VivHonorariosDetalle_Guardar(codEmpresa, usuario, request);
        }
    }
}
