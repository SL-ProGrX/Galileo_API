using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRemesasRetencionesBl
    {
        private readonly FrmCrRemesasRetencionesDb _db;

        public FrmCrRemesasRetencionesBl(IConfiguration config)
        {
            _db = new FrmCrRemesasRetencionesDb(config);
        }

        public ErrorDto<CrRemesasRetencionesPantallaData> Cr_RemesasRetenciones_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _db.Cr_RemesasRetenciones_Pantalla_Obtener(codEmpresa, usuario);

        public ErrorDto<CrRemesasRetencionesValidarData> Cr_RemesasRetenciones_Validar(
            int codEmpresa,
            CrRemesasRetencionesValidarRequest request)
            => _db.Cr_RemesasRetenciones_Validar(codEmpresa, request);

        public ErrorDto Cr_RemesasRetenciones_Aplicar(
            int codEmpresa,
            string usuario,
            CrRemesasRetencionesAplicarRequest request)
            => _db.Cr_RemesasRetenciones_Aplicar(codEmpresa, usuario, request);
    }
}
