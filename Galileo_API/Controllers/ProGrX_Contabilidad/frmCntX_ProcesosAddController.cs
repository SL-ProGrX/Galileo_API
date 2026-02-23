using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXProcesosAddController : ControllerBase
    {
        private readonly FrmCntXProcesosAddBl _bl;

        public FrmCntXProcesosAddController(IConfiguration config) => _bl = new FrmCntXProcesosAddBl(config);
        
        [HttpGet("CntXProcesosAdd_Obtener")]
        public ErrorDto<List<CtnXProcesosAddDto>> CntXProcesosAdd_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXProcesosAdd_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXProcesosAdd_Procesar")]
        public ErrorDto CntXProcesosAdd_Procesar(int codEmpresa, CntXProcesarRequest req)
        {
            return _bl.CntXProcesosAdd_Procesar(codEmpresa, req);
        }

    }
}