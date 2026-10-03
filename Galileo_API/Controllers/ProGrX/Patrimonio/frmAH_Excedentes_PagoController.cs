using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAHExcedentesPagoController : ControllerBase
    {
        private readonly FrmAHExcedentesPagoBL _bl;

        public FrmAHExcedentesPagoController(IConfiguration config)
        {
            _bl = new FrmAHExcedentesPagoBL(config);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Pago_Periodos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AH_Excedentes_Pago_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return _bl.AH_Excedentes_Pago_Periodos_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Separar_Casos_Aplicar")]
        public ErrorDto AH_Excedentes_Pago_Separar_Casos_Aplicar(int CodEmpresa, [FromBody] ExcSepararCasosRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Separar_Casos_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Casos_Especiales_Aplicar")]
        public ErrorDto AH_Excedentes_Pago_Casos_Especiales_Aplicar(int CodEmpresa, [FromBody] ExcCasosEspecialesRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Casos_Especiales_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Cuentas_Internas_Aplicar")]
        public ErrorDto<ExcPendientesDto> AH_Excedentes_Pago_Cuentas_Internas_Aplicar(int CodEmpresa, [FromBody] ExcAcreditarCuentasInternasRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Cuentas_Internas_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Tesoreria_Aplicar")]
        public ErrorDto AH_Excedentes_Pago_Tesoreria_Aplicar(int CodEmpresa, [FromBody] ExcTesoreriaRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Tesoreria_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Fondos_Aplicar")]
        public ErrorDto AH_Excedentes_Pago_Fondos_Aplicar(int CodEmpresa, [FromBody] ExcFondosRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Fondos_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Pago_Reclasificaciones_Aplicar")]
        public ErrorDto AH_Excedentes_Pago_Reclasificaciones_Aplicar(int CodEmpresa, [FromBody] ExcReclasificacionesRequestDto? request)
        {
            return _bl.AH_Excedentes_Pago_Reclasificaciones_Aplicar(CodEmpresa, request);
        }
    }
}