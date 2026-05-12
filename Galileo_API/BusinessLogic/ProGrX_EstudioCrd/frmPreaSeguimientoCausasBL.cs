using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaSeguimientoCausasBL
    {
        private readonly FrmPreaSeguimientoCausasDB _db;

        public FrmPreaSeguimientoCausasBL(IConfiguration config)
        {
            _db = new FrmPreaSeguimientoCausasDB(config);
        }

        public ErrorDto<FrmPreaSeguimientoCausasListaResponse> Prea_frmPreaSeguimientoCausas_Lista_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string tipo,
            string codigo)
        {
            return _db.Prea_frmPreaSeguimientoCausas_Lista_Obtener(
                codEmpresa,
                usuario,
                cod_preanalisis,
                tipo,
                codigo);
        }

        public ErrorDto<FrmPreaSeguimientoCausasRegistrarResponse> Prea_frmPreaSeguimientoCausas_Registrar(
            int codEmpresa,
            FrmPreaSeguimientoCausasRegistrarRequest request)
        {
            return _db.Prea_frmPreaSeguimientoCausas_Registrar(codEmpresa, request);
        }
    }
}
