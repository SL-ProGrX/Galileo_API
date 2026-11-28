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

        /// <summary>
        /// Obtiene la lista de expedientes con paginación server-side y filtro global.
        /// </summary>
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            FrmPreaConsultaExpeditentesFiltroRequest request)
        {
            return _db.Prea_frmPreaConsultaExpeditentes_Lista_Obtener(request);
        }

        public ErrorDto<List<FrmPreaConsultaExpeditentesEstado>> Prea_frmPreaConsultaExpeditentes_Estados_Obtener(
            int codEmpresa)
            => _db.Prea_frmPreaConsultaExpeditentes_Estados_Obtener(codEmpresa);
    }
}
