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
    public class FrmCntXTiposAsientosController : ControllerBase
    {
        private readonly FrmCntXTiposAsientosBl _bl;

        public FrmCntXTiposAsientosController(IConfiguration config) => _bl = new FrmCntXTiposAsientosBl(config);
        
        [HttpGet("CntXTiposAsientos_Obtener")]
        public ErrorDto<List<CntXTiposAsientosData>> CntXTiposAsientos_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXTiposAsientos_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXTiposAsientos_Guardar")]
        public ErrorDto CntXTiposAsientos_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposAsientosData request)
        {
            return _bl.CntXTiposAsientos_Guardar(codEmpresa, codConta, usuario, request);
        }

        [HttpDelete("CntXTiposAsientos_Eliminar")]
        public ErrorDto CntXTiposAsientos_Eliminar(int codEmpresa, int codConta, string usuario, string tipoAsiento)
        {
            return _bl.CntXTiposAsientos_Eliminar(codEmpresa, codConta, usuario, tipoAsiento);
        }

        [HttpPost("CntXTiposAsientos_Importar")]
        public ErrorDto CntXTiposAsientos_Importar(int codEmpresa, int codConta, string usuario)
        {
            return _bl.CntXTiposAsientos_Importar(codEmpresa, codConta, usuario);
        }
    }
}