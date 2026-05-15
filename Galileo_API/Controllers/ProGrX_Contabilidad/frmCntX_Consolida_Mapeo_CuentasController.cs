using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXConsolidaMapeoCuentasController : ControllerBase
    {
        private readonly FrmCntXConsolidaMapeoCuentasBL _bl;

        public FrmCntXConsolidaMapeoCuentasController(IConfiguration config)
        {
            _bl = new FrmCntXConsolidaMapeoCuentasBL(config);
        }

        [HttpGet("ConsolidaMapeoCuentas_ObtenerUnidades")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel?>>> ConsolidaMapeoCuentas_ObtenerUnidades(
            [FromQuery] int codEmpresa, 
            [FromQuery] int mContabilidad) 
            => _bl.ConsolidaMapeoCuentas_ObtenerUnidades(codEmpresa, mContabilidad);

        [HttpPost("ConsolidaMapeoCuentas_ImportaCargado")]
        public ActionResult<ErrorDto<ConsolidaMapeoImportaResultDto?>> ConsolidaMapeoCuentas_ImportaCargado(
            [FromQuery] int codEmpresa,
            [FromBody] ConsolidaMapeoImportaCargadoRequestDto request)
            => _bl.ConsolidaMapeoCuentas_ImportaCargado(codEmpresa, request);

        [HttpPost("ConsolidaMapeoCuentas_ImportaMapeo")]
        public ActionResult<ErrorDto<bool>> ConsolidaMapeoCuentas_ImportaMapeo(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_ImportaMapeo(codEmpresa, Consolidadora, Unidad, Usuario);

        [HttpGet("ConsolidaMapeoCuentas_ImportaResultados")]
        public ActionResult<ErrorDto<List<ConsolidaMapeoImportaResultadoDto?>>> ConsolidaMapeoCuentas_ImportaResultados(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_ImportaResultados(codEmpresa, Consolidadora, Unidad, Usuario);

        [HttpGet("ConsolidaMapeoCuentas_ImportaValida")]
        public ActionResult<ErrorDto<ConsolidaMapeoImportaValidaDto?>> ConsolidaMapeoCuentas_ImportaValida(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_ImportaValida(codEmpresa, Consolidadora, Unidad, Usuario);

        [HttpPost("ConsolidaMapeoCuentas_Importa")]
        public ActionResult<ErrorDto<ConsolidaMapeoImportaResultDto?>> ConsolidaMapeoCuentas_Importa(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_Importa(codEmpresa, Consolidadora, Unidad, Usuario);

        [HttpPost("ConsolidaMapeoCuentas_Inicializa")]
        public ActionResult<ErrorDto<ConsolidaMapeoImportaResultDto?>> ConsolidaMapeoCuentas_Inicializa(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_Inicializa(codEmpresa, Consolidadora, Unidad, Usuario);

        [HttpGet("ConsolidaMapeoCuentas_Actual")]
        public ActionResult<ErrorDto<List<ConsolidaMapeoActualDto?>>> ConsolidaMapeoCuentas_Actual(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Unidad)
            => _bl.ConsolidaMapeoCuentas_Actual(codEmpresa, Consolidadora, Unidad);

        [HttpGet("ConsolidaMapeoCuentas_ContabilidadInfo")]
        public ActionResult<ErrorDto<ConsolidaContabilidadDto?>> ConsolidaMapeoCuentas_ContabilidadInfo(
            [FromQuery] int codEmpresa,
            [FromQuery] int mContabilidad)
            => _bl.ConsolidaMapeoCuentas_ContabilidadInfo(codEmpresa, mContabilidad);

        [HttpPost("ConsolidaMapeoCuentas_ImportaContaBaseMapeo")]
        public ActionResult<ErrorDto<bool>> ConsolidaMapeoCuentas_ImportaContaBaseMapeo(
            [FromQuery] int codEmpresa,
            [FromQuery] int Consolidadora,
            [FromQuery] string Usuario)
            => _bl.ConsolidaMapeoCuentas_ImportaContaBaseMapeo(codEmpresa, Consolidadora, Usuario);
    }
}
