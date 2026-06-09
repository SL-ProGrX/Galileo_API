
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Galileo.Models.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPBancosAutorizadosController : ControllerBase
    {
        private readonly FrmCxPBancosAutorizadosBL CxP_BancosAutorizadosBL;

        public FrmCxPBancosAutorizadosController(IConfiguration config)
        {
            CxP_BancosAutorizadosBL = new FrmCxPBancosAutorizadosBL(config);
        }

        [HttpGet("ObtenerBancosAutorizados")]
        public ErrorDto<List<BancosAutorizadosDto>> ObtenerBancosAutorizados(int CodCliente)
        {
            return CxP_BancosAutorizadosBL.ObtenerBancosAutorizados(CodCliente);
        }

        [HttpGet("IngresarTesBancosNuevos")]
        public ErrorDto IngresarTesBancosNuevos(string Usuario, int CodCliente)
        {
            return CxP_BancosAutorizadosBL.IngresarTesBancosNuevos(Usuario, CodCliente);
        }

        [HttpGet("ActualizarCheque")]
        public ErrorDto ActualizarCheque(int BancoId, bool Valor, int CodCliente)
        {
            return CxP_BancosAutorizadosBL.ActualizarCheque(BancoId, Valor, CodCliente); 
        }

        [HttpGet("ActualizarTransferencia")]
        public ErrorDto ActualizarTransferencia(int BancoId, bool Valor, int CodCliente)
        {
            return CxP_BancosAutorizadosBL.ActualizarTransferencia(BancoId, Valor, CodCliente);
        }
    }
}