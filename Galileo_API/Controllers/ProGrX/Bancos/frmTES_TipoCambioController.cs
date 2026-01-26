using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesTipoCambioController : ControllerBase
    {

        private readonly FrmTesTipoCambioBL _bl;

        public FrmTesTipoCambioController(IConfiguration config)
        {
            _bl = new FrmTesTipoCambioBL(config);
        }

        [HttpGet("Tes_TipoCambio_Obtener")]
        public ErrorDto<TesTipoCambioDivisasTipoCambio> Tes_TipoCambio_Obtener(string tipoCambio)
        {
            return _bl.Tes_TipoCambio_Obtener(tipoCambio);
        }

        [HttpGet("Tes_TipoCambio_MontoCambiar")]
        public ErrorDto<double> Tes_TipoCambio_MontoCambiar(decimal pTipoCambio)
        {
            return _bl.Tes_TipoCambio_MontoCambiar(pTipoCambio);
        }

        [HttpGet("Tes_tipoCambioDivisa_Obterner")]
        public ErrorDto<string> Tes_tipoCambioDivisa_Obterner(int CodEmpresa, string cod_divisa)
        {
            return _bl.Tes_tipoCambioDivisa_Obterner(CodEmpresa, cod_divisa);
        }
    }
}
