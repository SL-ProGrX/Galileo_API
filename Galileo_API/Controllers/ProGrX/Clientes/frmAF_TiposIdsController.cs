using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFTiposIdsController : ControllerBase
    {
        private readonly FrmAFTiposIdsBL _bl;

        public FrmAFTiposIdsController(IConfiguration config)
        {
            _bl = new FrmAFTiposIdsBL(config);
        }

        [Authorize]
        [HttpGet("AF_TiposIds_Obtener")]
        public ErrorDto<AfTiposIdsLista> AF_TiposIds_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_TiposIds_Obtener(CodEmpresa, filtros);
        }
        
        [Authorize]
        [HttpPost("AF_TiposIds_Guardar")]
        public ErrorDto AF_TiposIds_Guardar(int CodEmpresa, string Usuario, AfTiposIdsDto Info)
        {
            return _bl.AF_TiposIds_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_TiposIds_Eliminar")]
        public ErrorDto AF_TiposIds_Eliminar(int CodEmpresa, string Usuario, int TipoId)
        {
            return _bl.AF_TiposIds_Eliminar(CodEmpresa, Usuario, TipoId);
        }
    }
}