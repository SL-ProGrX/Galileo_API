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
            int codEmpresa,
            string? buscar,
            int pagina,
            int paginacion)
        {
            return _db.Prea_frmPreaConsultaExpeditentes_Lista_Obtener(codEmpresa, buscar, pagina, paginacion);
        }
    }
}
