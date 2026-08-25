using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesTagsBl
    {
        private readonly FrmAfRecepcionDevolucionesTagsDb _db;

        public FrmAfRecepcionDevolucionesTagsBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmAfRecepcionDevolucionesTagsDb(config);
        }

        public ErrorDto<AfRecepcionDevolucionesTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesTags_Inicializar(int codEmpresa)
        {
            return _db.AF_frmAF_RecepcionDevolucionesTags_Inicializar(codEmpresa);
        }

        public ErrorDto<AfRecepcionDevolucionesTagsData?>
            AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _db.AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener(
                codEmpresa,
                cedula);
        }

        public ErrorDto<AfRecepcionDevolucionesTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesTagsAplicarRequest request)
        {
            return _db.AF_frmAF_RecepcionDevolucionesTags_Aplicar(
                codEmpresa,
                request);
        }
    }
}
