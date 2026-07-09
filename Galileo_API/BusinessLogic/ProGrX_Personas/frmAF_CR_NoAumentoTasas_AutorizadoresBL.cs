using Galileo_API.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrNoAumentoTasasAutorizadoresBL
    {
        private readonly FrmAFCrNoAumentoTasasAutorizadoresDB _db;

        public FrmAFCrNoAumentoTasasAutorizadoresBL(IConfiguration config)
        {
            _db = new FrmAFCrNoAumentoTasasAutorizadoresDB(config);
        }

        public ErrorDto<List<AfNatAutorizadores>> AF_NAT_Autorizadores_Obtener(int CodEmpresa)
        {
            return _db.AF_NAT_Autorizadores_Obtener(CodEmpresa);
        }

        public ErrorDto AF_NAT_Autorizadores_Asignar(int CodEmpresa, string A_Usuario, string Mov, string usuario)
        {
            return _db.AF_NAT_Autorizadores_Asignar(CodEmpresa, A_Usuario, Mov, usuario);
        }
    }
}
