using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAhExcedentesCeController : ControllerBase
    {
        private readonly FrmAhExcedentesCeBL _bl;

        public FrmAhExcedentesCeController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesCeBL(config);
        }

        [Authorize]
        [HttpGet("Excedentes_Periodos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Excedentes_Periodos_Lista([FromQuery] int codEmpresa)
        {
            return _bl.Excedentes_Periodos_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Excedentes_Periodo_Aplicaciones_Valida")]
        public ErrorDto<ExcedentesPeriodoValidaResult?> Excedentes_Periodo_Aplicaciones_Valida([FromQuery] int codEmpresa, [FromQuery] string periodoId)
        {
            return _bl.Excedentes_Periodo_Aplicaciones_Valida(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpGet("Excedentes_CasosEspeciales_Lista")]
        public ErrorDto<List<ExcedentesCasosEspecialesResult>> Excedentes_CasosEspeciales_Lista([FromQuery] int codEmpresa, [FromQuery] int lineas, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_CasosEspeciales_Lista(codEmpresa, lineas, periodoId);
        }

        [Authorize]
        [HttpGet("Excedentes_CasosEspecial_Nuevo_Lista")]
        public ErrorDto<List<ExcedentesCasosEspecialNuevoResult>> Excedentes_CasosEspecial_Nuevo_Lista([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_CasosEspecial_Nuevo_Lista(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpGet("Excedentes_CasosEspecial_Detalle")]
        public ErrorDto<ExcedentesCasosEspecialDetalleResult?> Excedentes_CasosEspecial_Detalle([FromQuery] int codEmpresa, [FromQuery] int periodoId, [FromQuery] string cedula)
        {
            return _bl.Excedentes_CasosEspecial_Detalle(codEmpresa, periodoId, cedula);
        }

        [Authorize]
        [HttpGet("Excedentes_CasosEspecial_SalidasCambio_Lista")]
        public ErrorDto<List<ExcedentesCasosEspecialSalidasCambioResult>> Excedentes_CasosEspecial_SalidasCambio_Lista([FromQuery] int codEmpresa)
        {
            return _bl.Excedentes_CasosEspecial_SalidasCambio_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Excedentes_Periodo_Estado")]
        public ErrorDto<ExcedentesPeriodoEstadoResult?> Excedentes_Periodo_Estado([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Periodo_Estado(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpPost("Excedentes_CasoEspecial_Add")]
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Add([FromQuery] int codEmpresa, [FromBody] ExcedentesCasoEspecialAddParams param)
        {
            return _bl.Excedentes_CasoEspecial_Add(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_CasoEspecial_Delete")]
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Delete([FromQuery] int codEmpresa, [FromBody] CasoEspecialBaseParams param)
        {
            return _bl.Excedentes_CasoEspecial_Delete(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CE_Sube")]
        public ErrorDto Excedentes_Mass_CE_Sube([FromQuery] int codEmpresa, [FromBody] ExcedentesMassCESubeParams param)
        {
            return _bl.Excedentes_Mass_CE_Sube(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CE_Valida")]
        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CE_Valida([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CE_Valida(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpGet("Excedentes_Mass_CE_Consulta")]
        public ErrorDto<List<ExcedentesMassConsultaBaseResult>> Excedentes_Mass_CE_Consulta([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CE_Consulta(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CE_Procesa")]
        public ErrorDto Excedentes_Mass_CE_Procesa([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CE_Procesa(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CS_Sube")]
        public ErrorDto Excedentes_Mass_CS_Sube([FromQuery] int codEmpresa, [FromBody] ExcedentesMassCSSubeParams param)
        {
            return _bl.Excedentes_Mass_CS_Sube(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CS_Valida")]
        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CS_Valida([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CS_Valida(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpGet("Excedentes_Mass_CS_Consulta")]
        public ErrorDto<List<ExcedentesMassCSConsultaResult>> Excedentes_Mass_CS_Consulta([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CS_Consulta(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpPost("Excedentes_Mass_CS_Procesa")]
        public ErrorDto Excedentes_Mass_CS_Procesa([FromQuery] int codEmpresa, [FromQuery] int periodoId)
        {
            return _bl.Excedentes_Mass_CS_Procesa(codEmpresa, periodoId);
        }

        [Authorize]
        [HttpPost("Excedentes_CasosEspeciales_Aplicados")]
        public ErrorDto<List<ExcedentesCasosEspecialesAplicadosResult>> Excedentes_CasosEspeciales_Aplicados([FromQuery] int codEmpresa, [FromBody] ExcedentesCasosEspecialesAplicadosParams param)
        {
            return _bl.Excedentes_CasosEspeciales_Aplicados(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_CambioSalida_Lista")]
        public ErrorDto<List<ExcedentesCambioSalidaListaResult>> Excedentes_CambioSalida_Lista([FromQuery] int codEmpresa, [FromBody] ExcedentesCambioSalidaListaParams param)
        {
            return _bl.Excedentes_CambioSalida_Lista(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Cambio_Salida_Add")]
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Add([FromQuery] int codEmpresa, [FromBody] ExcedentesCambioSalidaAddParams param)
        {
            return _bl.Excedentes_Cambio_Salida_Add(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Cambio_Salida_Delete")]
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Delete([FromQuery] int codEmpresa, [FromBody] ExcedentesCambioSalidaDeleteParams param)
        {
            return _bl.Excedentes_Cambio_Salida_Delete(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Excedentes_Cambio_Salida_Autoriza")]
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Autoriza([FromQuery] int codEmpresa, [FromBody] CasoEspecialBaseParams param)
        {
            return _bl.Excedentes_Cambio_Salida_Autoriza(codEmpresa, param);
        }
    }
}
