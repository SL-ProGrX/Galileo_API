using PgxAPI.DataBaseTier;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;


namespace PgxAPI.BusinessLogic
{
    public class mCntLinkBL
    {
        private readonly IConfiguration _config;

        public mCntLinkBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<DefMascarasDto> sbgCntParametros(int CodEmpresa)
        {
            return new MCntLinkDB(_config).sbgCntParametros(CodEmpresa);
        }

        public string fxgCntCuentaFormato(int CodEmpresa, bool blnMascara, string pCuenta, int optMensaje = 1)
        {
            return new MCntLinkDB(_config).fxgCntCuentaFormato(CodEmpresa, blnMascara, pCuenta, optMensaje);
        }

        public bool fxgCntCuentaValida(int CodEmpresa, string vCuenta)
        {
            return new MCntLinkDB(_config).fxgCntCuentaValida(CodEmpresa, vCuenta);
        }



    }
}
