using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoCobroFiadoresAplicacionController : ControllerBase
    {
        private readonly FrmCoCobroFiadoresAplicacionBL _bl;

        public FrmCoCobroFiadoresAplicacionController(IConfiguration config)
        {
            _bl = new FrmCoCobroFiadoresAplicacionBL(config);
        }

        [Authorize]
        [HttpGet("CO_CobroFiadores_Aplicacion_Pendientes_Obtener")]
        public ErrorDto<CoCobroFiadoresAplicacionPendientesDto> CO_CobroFiadores_Aplicacion_Pendientes_Obtener(int CodEmpresa)
        {
            return _bl.CO_CobroFiadores_Aplicacion_Pendientes_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("CO_CobroFiadores_Aplicacion_Procesar")]
        public ErrorDto<CoCobroFiadoresAplicacionProcesarResponse> CO_CobroFiadores_Aplicacion_Procesar(int CodEmpresa,[FromBody] CoCobroFiadoresAplicacionProcesarRequest data)
        {
            return _bl.CO_CobroFiadores_Aplicacion_Procesar(CodEmpresa, data);
        }

        [Authorize]
        [HttpPost("CO_CobroFiadores_Aplicacion_Cancelar")]
        public ErrorDto CO_CobroFiadores_Aplicacion_Cancelar(int CodEmpresa,[FromBody] CoCobroFiadoresAplicacionCancelarRequest data)
        {
            return _bl.CO_CobroFiadores_Aplicacion_Cancelar(CodEmpresa, data);
        }
    }
}