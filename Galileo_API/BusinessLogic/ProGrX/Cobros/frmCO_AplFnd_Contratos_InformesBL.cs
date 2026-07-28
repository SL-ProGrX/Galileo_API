using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndContratosInformesModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndContratosInformesBL
    {
        private readonly FrmCoAplFndContratosInformesDB _db;

        public FrmCoAplFndContratosInformesBL(IConfiguration config)
        {
            _db = new FrmCoAplFndContratosInformesDB(config);
        }

        public ErrorDto<List<CoAplFndContratosInformesPersonasResult>> Co_AplFnd_ContratosInformes_Personas_Obtener(int codEmpresa)
                => _db.Co_AplFnd_ContratosInformes_Personas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_ContratosInformes_Aplicaciones_Obtener(int codEmpresa)
               => _db.Co_AplFnd_ContratosInformes_Aplicaciones_Obtener(codEmpresa);

        public ErrorDto Co_AplFnd_ContratosInformes_Bitacora_Registrar(int codEmpresa, string usuario, string strTipoMovimiento, string strDetalleMovimiento)
             => _db.Co_AplFnd_ContratosInformes_Bitacora_Registrar(codEmpresa, usuario, strTipoMovimiento, strDetalleMovimiento);
    }
}
