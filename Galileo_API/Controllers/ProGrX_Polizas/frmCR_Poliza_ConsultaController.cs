using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRPolizaConsultaController : ControllerBase
    {
        private readonly FrmCRPolizaConsultaBL _bl;

        public FrmCRPolizaConsultaController(IConfiguration config)
        {
            _bl = new FrmCRPolizaConsultaBL(config);
        }

        [Authorize]
        [HttpPost("Poliza_Persona_Filtros_Lista")]
        public ErrorDto<List<PolizaPersonaFiltroDto>> Poliza_Persona_Filtros_Lista(int codEmpresa, [FromBody] PolizaPersonaFiltroParams param)
            => _bl.Poliza_Persona_Filtros_Lista(codEmpresa, param);

        [Authorize]
        [HttpPost("Poliza_Persona_Creditos")]
        public ErrorDto<List<PolizaPersonaCreditoDto>> Poliza_Persona_Creditos(int codEmpresa, [FromBody] PolizaPersonaCreditoParams param)
            => _bl.Poliza_Persona_Creditos(codEmpresa, param);

        [Authorize]
        [HttpPost("Poliza_Persona_Operaciones_Polizas")]
        public ErrorDto<List<PolizaPersonaOperacionPolizaDto>> Poliza_Persona_Operaciones_Polizas(int codEmpresa, [FromBody] PolizaPersonaOperacionPolizaParams param)
            => _bl.Poliza_Persona_Operaciones_Polizas(codEmpresa, param);

        [Authorize]
        [HttpPost("Poliza_Persona_Reclamos")]
        public ErrorDto<List<PolizaPersonaReclamoDto>> Poliza_Persona_Reclamos(int codEmpresa, [FromBody] PolizaPersonaReclamoParams param)
            => _bl.Poliza_Persona_Reclamos(codEmpresa, param);
    }
}
