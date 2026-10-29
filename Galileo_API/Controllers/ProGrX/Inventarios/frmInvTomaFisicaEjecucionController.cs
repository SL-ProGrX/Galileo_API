using Microsoft.AspNetCore.Mvc;
using Galileo_API.Extensions;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTomaFisicaEjecucionController : ControllerBase
    {
        private readonly FrmInvTomaFisicaEjecucionBL _bl;
        public FrmInvTomaFisicaEjecucionController(IConfiguration config)
        {
            _bl = new FrmInvTomaFisicaEjecucionBL(config);
        }

        [HttpGet("Obtener_Entradas")]
        public ErrorDto<List<EntradasTomaFisicaDto>> Obtener_Entradas(int CodEmpresa)
        {
            return _bl.Obtener_Entradas(CodEmpresa);
        }

        [HttpGet("Obtener_Salidas")]
        public ErrorDto<List<SalidasTomaFisicaDto>> Obtener_Salidas(int CodEmpresa)
        {
            return _bl.Obtener_Salidas(CodEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ProcesarTomaFisica")]
        public ErrorDto ProcesarTomaFisica(int CodEmpresa, int consecutivo, string usuario, string cod_entrada, string cod_salida)
        {
            return _bl.ProcesarTomaFisica(CodEmpresa, consecutivo, User.ObtenerUsuario(), cod_entrada, cod_salida);
        }
    }
}