using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesReposicionBL
    {
        private readonly FrmTesReposicionDB _reposicionDb;

        public FrmTesReposicionBL(IConfiguration config)
        {
            _reposicionDb = new FrmTesReposicionDB(config);
        }

        public ErrorDto<TesReposicionData> TES_Reposicion_Obtener(int CodEmpresa, int solicitud)
        {
            return _reposicionDb.TES_Reposicion_Obtenet(CodEmpresa, solicitud);
        }

        public ErrorDto TES_Reposicion_Guardar(int CodEmpresa, TesReposicionData solicitud)
        {
            return _reposicionDb.TES_Reposicion_Guardar(CodEmpresa, solicitud);
        }
    }
}
