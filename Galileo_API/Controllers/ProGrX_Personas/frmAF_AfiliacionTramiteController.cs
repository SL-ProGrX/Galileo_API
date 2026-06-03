using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrx_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfAfiliacionTramiteController : ControllerBase
    {
        private readonly IConfiguration? _config;
        private readonly FrmAfAfiliacionTramiteBl BlAfAfiliacionTramite;

        public FrmAfAfiliacionTramiteController(IConfiguration config)
        {
            _config = config;
            BlAfAfiliacionTramite = new FrmAfAfiliacionTramiteBl(_config);
        }

        [Authorize]
        [HttpGet("AF_AfiliacionTramite_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_AfiliacionTramite_Instituciones_Obtener(int CodEmpresa)
        {
            return BlAfAfiliacionTramite.AF_AfiliacionTramite_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_AfiliacionTramite_Obtener")]
        public ErrorDto<List<AfAfiliacionTramiteDto>> AF_AfiliacionTramite_Obtener(int CodEmpresa, string Filtros)
        {
            return BlAfAfiliacionTramite.AF_AfiliacionTramite_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpPost("AF_AfiliacionTramite_Aprobar")]
        public ErrorDto AF_AfiliacionTramite_Aprobar(int CodEmpresa, List<AfAfiliacionTramiteDto> Lista, string Usuario)
        {
            return BlAfAfiliacionTramite.AF_AfiliacionTramite_Aprobar(CodEmpresa, Lista, Usuario);
        }
    }
}
