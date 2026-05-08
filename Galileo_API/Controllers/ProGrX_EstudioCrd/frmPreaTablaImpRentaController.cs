using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaTablaImpRentaController : ControllerBase
    {
        private readonly FrmPreaTablaImpRentaBl _bl;

        public FrmPreaTablaImpRentaController(IConfiguration config) =>
            _bl = new FrmPreaTablaImpRentaBl(config);

        [HttpGet("CrPreaTablaImpRenta_Obtener")]
        public ErrorDto<List<CrdPreaTablaImpRentaData>> CrPreaTablaImpRenta_Obtener(int codEmpresa)
        {
            return _bl.CrPreaTablaImpRenta_Obtener(codEmpresa);
        }

        [HttpPost("CrPreaTablaImpRenta_Guardar")]
        public ErrorDto CrPreaTablaImpRenta_Guardar(int codEmpresa, string usuario, CrdPreaTablaImpRentaData request)
        {
            return _bl.CrPreaTablaImpRenta_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrPreaTablaImpRenta_Eliminar")]
        public ErrorDto CrPreaTablaImpRenta_Eliminar(int codEmpresa, int idx, string usuario)
        {
            return _bl.CrPreaTablaImpRenta_Eliminar(codEmpresa, idx, usuario);
        }
    }
}
