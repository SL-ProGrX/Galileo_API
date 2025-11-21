using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class MCntLinkBl
    {
        private readonly IConfiguration _config;

        public MCntLinkBl(IConfiguration config)
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
