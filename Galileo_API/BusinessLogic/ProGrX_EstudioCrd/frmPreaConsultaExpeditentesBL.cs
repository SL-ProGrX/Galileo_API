using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaConsultaExpeditentesBL
    {
        private readonly FrmPreaConsultaExpeditentesDB _db;

        public FrmPreaConsultaExpeditentesBL(IConfiguration configuration)
        {
            _db = new FrmPreaConsultaExpeditentesDB(configuration);
        }

        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            int codEmpresa,
            string? buscar)
        {
            return _db.Prea_frmPreaConsultaExpeditentes_Lista_Obtener(codEmpresa, buscar);
        }
    }
}
