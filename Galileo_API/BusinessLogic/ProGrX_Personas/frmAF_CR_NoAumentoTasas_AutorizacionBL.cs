using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrNoAumentoTasasAutorizacionBL
    {
        private readonly FrmAFCrNoAumentoTasasAutorizacionDB _db;

        public FrmAFCrNoAumentoTasasAutorizacionBL(IConfiguration config)
        {
            _db = new FrmAFCrNoAumentoTasasAutorizacionDB(config);
        }

        public ErrorDto<List<AfNatAutorizacion>> AF_NAT_Autorizacion_Obtener(int CodEmpresa, AfNatAutorizacionFiltros Filtro)
        {
            return _db.AF_NAT_Autorizacion_Obtener(CodEmpresa, Filtro);
        }

        public ErrorDto AF_NAT_Autorizacion_Autorizar(int CodEmpresa, int RenunciaId, string usuario)
        {
            return _db.AF_NAT_Autorizacion_Autorizar(CodEmpresa, RenunciaId, usuario);
        }
    }
}
