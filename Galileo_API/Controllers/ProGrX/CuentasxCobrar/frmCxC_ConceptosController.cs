using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCConceptosController : ControllerBase
    {
        private readonly FrmCxCConceptosBL _bl;

        public FrmCxCConceptosController(IConfiguration config)
        {
            _bl = new FrmCxCConceptosBL(config);
        }

        [Authorize]
        [HttpGet("CxcConceptos_Lista")]
        public ErrorDto<List<CxcConceptoDto>> CxcConceptos_Lista(int codEmpresa)
        {
            return _bl.CxcConceptos_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxcConceptos_Existe")]
        public ErrorDto<CxcConceptoExisteResult?> CxcConceptos_Existe(int codEmpresa, [FromQuery] string codigo)
        {
            return _bl.CxcConceptos_Existe(codEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("CxcConceptos_Guardar")]
        public ErrorDto<bool> CxcConceptos_Guardar(int codEmpresa, [FromBody] CxcConceptoSaveParams param)
        {
            return _bl.CxcConceptos_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcConceptos_Eliminar")]
        public ErrorDto<bool> CxcConceptos_Eliminar(int codEmpresa, [FromBody] CxcConceptoDeleteParams param)
        {
            return _bl.CxcConceptos_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcConceptos_ListaGenerica")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxcConceptos_ListaGenerica(int codEmpresa)
        {
            return _bl.CxcConceptos_ListaGenerica(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxcConceptos_ContratosAsignados")]
        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_ContratosAsignados(int codEmpresa, [FromQuery] string codConcepto)
        {
            return _bl.CxcConceptos_ContratosAsignados(codEmpresa, codConcepto);
        }

        [Authorize]
        [HttpGet("CxcConceptos_FacturaEstadosAsignados")]
        public ErrorDto<List<CxcConceptoAsignacionDto>> CxcConceptos_FacturaEstadosAsignados(int codEmpresa, [FromQuery] string codConcepto)
        {
            return _bl.CxcConceptos_FacturaEstadosAsignados(codEmpresa, codConcepto);
        }

        [Authorize]
        [HttpPost("CxcConceptos_Contrato_Insertar")]
        public ErrorDto<bool> CxcConceptos_Contrato_Insertar(int codEmpresa, [FromBody] CxcConceptoContratoParams param)
        {
            return _bl.CxcConceptos_Contrato_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcConceptos_Contrato_Eliminar")]
        public ErrorDto<bool> CxcConceptos_Contrato_Eliminar(int codEmpresa, [FromBody] CxcConceptoContratoParams param)
        {
            return _bl.CxcConceptos_Contrato_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcConceptos_FacturaEstado_Insertar")]
        public ErrorDto<bool> CxcConceptos_FacturaEstado_Insertar(int codEmpresa, [FromBody] CxcConceptoFacturaEstadoParams param)
        {
            return _bl.CxcConceptos_FacturaEstado_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcConceptos_FacturaEstado_Eliminar")]
        public ErrorDto<bool> CxcConceptos_FacturaEstado_Eliminar(int codEmpresa, [FromBody] CxcConceptoFacturaEstadoParams param)
        {
            return _bl.CxcConceptos_FacturaEstado_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcPersonas_Pagadores")]
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Pagadores(int codEmpresa)
        {
            return _bl.CxcPersonas_Pagadores(codEmpresa);
        }

        [Authorize]
        [HttpPost("CxcConceptos_ActualizarPagadorDefault")]
        public ErrorDto<bool> CxcConceptos_ActualizarPagadorDefault(int codEmpresa, [FromBody] CxcConceptoPagadorDefaultParams param)
        {
            return _bl.CxcConceptos_ActualizarPagadorDefault(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("Unidades_ListaPorContabilidad")]
        public ErrorDto<List<UnidadDto>> Unidades_ListaPorContabilidad(int codEmpresa, [FromQuery] string codContabilidad)
        {
            return _bl.Unidades_ListaPorContabilidad(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet("CentrosCosto_ListaPorContabilidad")]
        public ErrorDto<List<CentrosCostoDto>> CentrosCosto_ListaPorContabilidad(int codEmpresa, [FromQuery] string codContabilidad)
        {
            return _bl.CentrosCosto_ListaPorContabilidad(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpPost("CxcConceptos_Incobrable")]
        public ErrorDto<bool> CxcConceptos_Incobrable(int codEmpresa, [FromBody] CxcConceptoIncobrableParams param)
        {
            return _bl.CxcConceptos_Incobrable(codEmpresa, param);
        }
    }
}
