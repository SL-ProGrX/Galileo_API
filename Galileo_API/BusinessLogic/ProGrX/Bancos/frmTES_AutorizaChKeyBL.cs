using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesAutorizaChKeyBL
    {
        private readonly FrmTesAutorizaChKeyDB _dbAuthChKey;

        public FrmTesAutorizaChKeyBL(IConfiguration config)
        {
            _dbAuthChKey = new FrmTesAutorizaChKeyDB(config);
        }

        public ErrorDto Tes_AutorizaChKey_Cambiar(AutorizaChKeyData usuario)
        {
            return _dbAuthChKey.Tes_AutorizaChKey_Cambiar(usuario);
        }
    }
}
