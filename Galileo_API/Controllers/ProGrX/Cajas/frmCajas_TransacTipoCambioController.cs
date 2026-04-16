using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
 
namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasTransacTipoCambioController : ControllerBase
    {
        private readonly FrmCajasTransacTipoCambioBL _bl;
        public FrmCajasTransacTipoCambioController(IConfiguration config)
        {
            _bl = new FrmCajasTransacTipoCambioBL(config);
        }

        [Authorize]
        [HttpGet("Cajas_TransacTipoCambio_TipoDocumento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_TipoDocumento_Obtener(int CodEmpresa, string Caja = "")
        {
            return _bl.Cajas_TransacTipoCambio_TipoDocumento_Obtener(CodEmpresa, Caja);
            
        }

        [Authorize]
        [HttpGet("Cajas_TransacTipoCambio_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_Divisas_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_TransacTipoCambio_Divisas_Obtener(CodEmpresa);

        }
    }
}