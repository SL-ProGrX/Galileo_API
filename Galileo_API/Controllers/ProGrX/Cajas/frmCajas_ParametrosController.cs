using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasParametrosController : ControllerBase
    {
        private readonly FrmCajasParametrosBL _bl;
        public FrmCajasParametrosController(IConfiguration config)
        {
            _bl = new FrmCajasParametrosBL(config);
        }

        [Authorize]
        [HttpGet("Cajas_Parametros_Lista_Obtener")]
        public ErrorDto<List<CajasParametrosData>> Cajas_Parametros_Lista_Obtener( int CodEmpresa, string filtros)
        {
            return _bl.Cajas_Parametros_Lista_Obtener(CodEmpresa, filtros);
            
        }

        [Authorize]
        [HttpPost("Cajas_Parametros_Guardar")]
        public ErrorDto Cajas_Parametros_Guardar( int CodEmpresa,  CajasParametrosData parametro)
        {
            return _bl.Cajas_Parametros_Guardar(CodEmpresa, parametro);
            
        }
    }
}