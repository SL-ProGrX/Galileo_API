using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFTiposSociedadesController : ControllerBase
    {
        private readonly FrmAFTiposSociedadesBL _bl;

        public FrmAFTiposSociedadesController(IConfiguration config)
        {
            _bl = new FrmAFTiposSociedadesBL(config);
        }

        [Authorize]
        [HttpGet("AF_TiposSociedades_Obtener")]
        public ErrorDto<AfTiposSociedadesLista> AF_TiposSociedades_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_TiposSociedades_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_TiposSociedades_Guardar")]
        public ErrorDto AF_TiposSociedades_Guardar(int CodEmpresa, string Usuario, AfTiposSociedadesDto Info)
        {
            return _bl.AF_TiposSociedades_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_TiposSociedades_Eliminar")]
        public ErrorDto AF_TiposSociedades_Eliminar(int CodEmpresa, string Usuario, string CodSociedad)
        {
            return _bl.AF_TiposSociedades_Eliminar(CodEmpresa, Usuario, CodSociedad);
        }
    }
}