using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPEventosVentasController : ControllerBase
    {
        private readonly FrmCxPEventosVentasBL Eventos_VentasBL;
        public FrmCxPEventosVentasController(IConfiguration config)
        {
            Eventos_VentasBL = new FrmCxPEventosVentasBL(config);
        }

        [HttpGet("Eventos_Obtener")]
        public ErrorDto<List<CxpEventosDto>> Eventos_Obtener(int CodEmpresa)
        {
            return Eventos_VentasBL.Eventos_Obtener(CodEmpresa);
        }

        [HttpGet("Eventos_Ventas_Obtener")]
        public ErrorDto<List<CxpEventosVentasDto>> Eventos_Ventas_Obtener(int CodEmpresa, string parametros)
        {
            return Eventos_VentasBL.Eventos_Ventas_Obtener(CodEmpresa, parametros);
        }
    }
}