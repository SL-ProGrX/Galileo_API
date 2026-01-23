using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesCopiaEsquemaController : ControllerBase
    {
        private readonly FrmTesCopiaEsquemaBL _copiaEsquemaBL;

        public FrmTesCopiaEsquemaController(IConfiguration config)
        {
            _copiaEsquemaBL = new FrmTesCopiaEsquemaBL(config);
        }

        
        [HttpGet("Tes_CopiaEsquema_Obtener")]
        public ErrorDto<TesCopiaEsquemaModels> Tes_CopiaEsquema_Obtener(int CodEmpresa, int solicitud, int contabilidad)
        {
            return _copiaEsquemaBL.Tes_CopiaEsquema_Obtener(CodEmpresa, solicitud, contabilidad);
        }

        [Authorize]
        [HttpPost("Tes_CopiarEsquema_Guardar")]
        public ErrorDto Tes_CopiarEsquema_Guardar(int CodEmpresa, TesCopiaEsquemaModels solicitud)
        {
            return _copiaEsquemaBL.Tes_CopiarEsquema_Guardar(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("Tes_CopiaEsquemaLista_Obtener")]
        public ErrorDto<TesCopiaEsquemaLista> Tes_CopiaEsquemaLista_Obtener(int CodEmpresa, int contabilidad, string filtros)
        {
            return _copiaEsquemaBL.Tes_CopiaEsquemaLista_Obtener(CodEmpresa, contabilidad, filtros);
        }

    }
}
