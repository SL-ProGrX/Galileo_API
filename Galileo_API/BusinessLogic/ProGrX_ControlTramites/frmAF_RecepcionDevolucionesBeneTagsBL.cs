using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesBeneTagsBl
    {
        private readonly FrmAfRecepcionDevolucionesBeneTagsDb _db;

        public FrmAfRecepcionDevolucionesBeneTagsBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmAfRecepcionDevolucionesBeneTagsDb(config);
        }

        public ErrorDto<AfRecepcionDevolucionesBeneTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar(int codEmpresa)
        {
            return _db.AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar(codEmpresa);
        }

        public ErrorDto<AfRecepcionDevolucionesBeneTagsData?>
            AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener(
                int codEmpresa,
                string codBeneficio,
                string codigo)
        {
            return _db.AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener(
                codEmpresa,
                codBeneficio,
                codigo);
        }

        public ErrorDto<AfRecepcionDevolucionesBeneTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesBeneTagsAplicarRequest request)
        {
            return _db.AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar(
                codEmpresa,
                request);
        }
    }
}
