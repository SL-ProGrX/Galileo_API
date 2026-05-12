using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaAbandonaMotivosBL
    {
        private readonly FrmPreaAbandonaMotivosDB _db;

        public FrmPreaAbandonaMotivosBL(IConfiguration config)
        {
            _db = new FrmPreaAbandonaMotivosDB(config);
        }

        public ErrorDto<FrmPreaAbandonaMotivosListaResponse> Prea_frmPreaAbandonaMotivos_Lista_Obtener(
            int codEmpresa,
             string usuario,
             string cod_preanalisis)
        {
            return _db.Prea_frmPreaAbandonaMotivos_Lista_Obtener(codEmpresa, usuario, cod_preanalisis);
        }

        public ErrorDto<FrmPreaAbandonaMotivosRegistrarResponse> Prea_frmPreaAbandonaMotivos_Registrar(
            int codEmpresa,
            FrmPreaAbandonaMotivosRegistrarRequest request)
        {
            return _db.Prea_frmPreaAbandonaMotivos_Registrar(codEmpresa, request);
        }
    }
}
