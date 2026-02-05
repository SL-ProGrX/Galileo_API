using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCContratosController : ControllerBase
    {
        private readonly FrmCxCContratosBL _bl;

        public FrmCxCContratosController(IConfiguration config)
        {
            _bl = new FrmCxCContratosBL(config);
        }

        [Authorize]
        [HttpGet("Contratos_Busqueda_Lista")]
        public ErrorDto<List<ContratoBusquedaDto>> Contratos_Busqueda_Lista(int codEmpresa)
            => _bl.Contratos_Busqueda_Lista(codEmpresa);

        [Authorize]
        [HttpGet("Contrato_ObtenerPorCodigo")]
        public ErrorDto<ContratoDetalleDto?> Contrato_ObtenerPorCodigo(int codEmpresa, [FromQuery] string codContrato)
            => _bl.Contrato_ObtenerPorCodigo(codEmpresa, codContrato);

        [Authorize]
        [HttpGet("Contrato_PersonasPorContrato")]
        public ErrorDto<List<ContratoPersonaDto>> Contrato_PersonasPorContrato(int codEmpresa, [FromQuery] string codContrato)
            => _bl.Contrato_PersonasPorContrato(codEmpresa, codContrato);

        [Authorize]
        [HttpPost("Contrato_PersonaPagador_Eliminar")]
        public ErrorDto<bool> Contrato_PersonaPagador_Eliminar(int codEmpresa, [FromBody] ContratoPersonaDeleteParams param)
            => _bl.Contrato_PersonaPagador_Eliminar(codEmpresa, param);

        [Authorize]
        [HttpPost("Contrato_PersonaSuscripcion_Eliminar")]
        public ErrorDto<bool> Contrato_PersonaSuscripcion_Eliminar(int codEmpresa, [FromBody] ContratoPersonaDeleteParams param)
            => _bl.Contrato_PersonaSuscripcion_Eliminar(codEmpresa, param);

        [Authorize]
        [HttpPost("Contrato_Persona_Eliminar")]
        public ErrorDto<bool> Contrato_Persona_Eliminar(int codEmpresa, [FromBody] ContratoPersonaDeleteParams param)
            => _bl.Contrato_Persona_Eliminar(codEmpresa, param);

        [Authorize]
        [HttpGet("Contrato_PagadoresPorContrato")]
        public ErrorDto<List<ContratoPagadorDto>> Contrato_PagadoresPorContrato(int codEmpresa, [FromQuery] string codContrato)
            => _bl.Contrato_PagadoresPorContrato(codEmpresa, codContrato);

        [Authorize]
        [HttpPost("Contrato_Pagador_Eliminar")]
        public ErrorDto<bool> Contrato_Pagador_Eliminar(int codEmpresa, [FromBody] ContratoPersonaDeleteParams param)
            => _bl.Contrato_Pagador_Eliminar(codEmpresa, param);

        [Authorize]
        [HttpGet("Contrato_CargosPorContrato")]
        public ErrorDto<List<ContratoCargoDto>> Contrato_CargosPorContrato(int codEmpresa, [FromQuery] string codContrato)
            => _bl.Contrato_CargosPorContrato(codEmpresa, codContrato);

        [Authorize]
        [HttpPost("Contrato_Cargo_Eliminar")]
        public ErrorDto<bool> Contrato_Cargo_Eliminar(int codEmpresa, [FromBody] ContratoCargoDeleteParams param)
            => _bl.Contrato_Cargo_Eliminar(codEmpresa, param);

        [Authorize]
        [HttpGet("Contrato_ConceptosPorContrato")]
        public ErrorDto<List<ContratoConceptoDto>> Contrato_ConceptosPorContrato(int codEmpresa, [FromQuery] string codContrato)
            => _bl.Contrato_ConceptosPorContrato(codEmpresa, codContrato);

        [Authorize]
        [HttpPost("Contrato_Concepto_Insertar")]
        public ErrorDto<bool> Contrato_Concepto_Insertar(int codEmpresa, [FromBody] ContratoConceptoParams param)
            => _bl.Contrato_Concepto_Insertar(codEmpresa, param);

        [Authorize]
        [HttpPost("Contrato_Concepto_Eliminar")]
        public ErrorDto<bool> Contrato_Concepto_Eliminar(int codEmpresa, [FromBody] ContratoConceptoParams param)
            => _bl.Contrato_Concepto_Eliminar(codEmpresa, param);
    }
}
