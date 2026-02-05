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
    public class FrmCntXCierresController : ControllerBase
    {
        private readonly FrmCntXCierresBl _bl;

        public FrmCntXCierresController(IConfiguration config) => _bl = new FrmCntXCierresBl(config);

        [HttpGet("CntXCierres_Obtener")]
        public ErrorDto<List<CntXCierreData>> CntXCierres_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXCierres_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXCierres_Guardar")]
        public ErrorDto CntXCierres_Guardar(int codEmpresa, int codConta, string usuario, CntXCierreData request)
        {
            return _bl.CntXCierres_Guardar(codEmpresa, codConta, usuario, request);
        }

        [HttpDelete("CntXCierres_Eliminar")]
        public ErrorDto CntXCierres_Eliminar(int codEmpresa, int codConta, string usuario, int idCierre)
        {
            return _bl.CntXCierres_Eliminar(codEmpresa, codConta, usuario, idCierre);
        }
    }
}