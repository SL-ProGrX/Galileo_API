using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFPersonaTarjetasController : ControllerBase
    {
        private readonly FrmAFPersonaTarjetasBL BL_AF_PersonaTarjetas;
        public FrmAFPersonaTarjetasController(IConfiguration config)
        {
            BL_AF_PersonaTarjetas = new FrmAFPersonaTarjetasBL(config);
        }

        [Authorize]
        [HttpGet("AF_PersonaTarjetas_Consulta")]
        public ErrorDto<List<PersonaTarjetaDto>> AF_PersonaTarjetas_Consulta(int codEmpresa, string cedula)
        {
            return BL_AF_PersonaTarjetas.AF_PersonaTarjetas_Consulta(codEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_PersonaTarjetas_Registro")]
        public ErrorDto AF_PersonaTarjetas_Registro(int CodEmpresa, PersonaTarjetaRegistroDto tarjeta)
        {
            return BL_AF_PersonaTarjetas.AF_PersonaTarjetas_Registro(CodEmpresa, tarjeta);
        }

        [Authorize]
        [HttpGet("AF_PersonaTarjetas_ValidaTipo")]
        public ErrorDto<string> AF_PersonaTarjetas_ValidaTipo(string Tarjeta)
        {
            return FrmAFPersonaTarjetasBL.AF_PersonaTarjetas_ValidaTipo(Tarjeta);
        }
    }
}