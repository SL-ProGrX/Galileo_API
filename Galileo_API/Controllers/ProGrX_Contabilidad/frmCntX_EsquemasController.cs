using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.BusinessTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCntXEsquemasController : ControllerBase
    {
        private readonly FrmCntXEsquemasBl _bl;

        public FrmCntXEsquemasController(IConfiguration config)
        {
            _bl = new FrmCntXEsquemasBl(config);
        }

        [Authorize]
        [HttpGet("ObtenerContabilidades")]
        public ErrorDto<List<ContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            return _bl.ObtenerContabilidades(codEmpresa);
        }
        [Authorize]
        [HttpPost("Copiar")]
        public ErrorDto Copiar(int codEmpresa,int codFuente,int codDestino,bool inicializa,string usuario)
        {
            return _bl.Copiar(codEmpresa,codFuente,codDestino,inicializa,usuario);
        }


    }
}