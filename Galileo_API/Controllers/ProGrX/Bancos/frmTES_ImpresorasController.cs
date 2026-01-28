using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic;

namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesImpresorasController : ControllerBase
    {
        private readonly FrmTesImpresorasBL _bl;

        public FrmTesImpresorasController(IConfiguration config)
        {
            _bl = new FrmTesImpresorasBL(config);
        }


        
        [HttpPost("TES_Impresoras_Guardar")]
        public ErrorDto Tes_Impresoras_Guardar(int CodEmpresa, string usuario, TesImpresorasDto impresora)
        {
            return _bl.Tes_Impresoras_Guardar(CodEmpresa, usuario, impresora);
        }

        [HttpGet("Tes_Impresoras_Obtener")]
        public ErrorDto<TesImpresorasDto> Tes_Impresoras_Obtener(int CodEmpresa)
        {
            return _bl.Tes_Impresoras_Obtener(CodEmpresa);
        }

    }
}
