
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasCorreccionesModels;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCCuentasCorreccionesController : ControllerBase
    {
        private readonly FrmCxCCuentasCorreccionesBL _bl;

        public FrmCxCCuentasCorreccionesController(IConfiguration config)
            => _bl = new FrmCxCCuentasCorreccionesBL(config);

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesBancos_Obtener")]       
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesBancos_Obtener(int CodEmpresa)
        {
            return _bl.CxC_CuentasCorreccionesBancos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesCuentasBancarias_Obtener")] 
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesCuentasBancarias_Obtener(int CodEmpresa, string cedula, string codBanco)
        {
            return _bl.CxC_CuentasCorreccionesCuentasBancarias_Obtener(CodEmpresa, cedula, codBanco);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesAutorizado_Consultar")] 
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesAutorizado_Consultar(int codEmpresa, string cedula, int orden, string cedulaAutorizado="")
        {
            return _bl.CxC_CuentasCorreccionesAutorizado_Consultar(codEmpresa, cedula, cedulaAutorizado, orden);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesContrato_Consultar")] 
        public ErrorDto<ContratoData> CxC_CuentasCorreccionesContrato_Consultar(int codEmpresa, int orden, string cedula, string concepto, string contrato = "")
        {
            return _bl.CxC_CuentasCorreccionesContrato_Consultar(codEmpresa, orden, cedula, concepto, contrato);
        }
        [Authorize]
        [HttpGet("CxC_ContratoDetalle_Consultar")]
        public ErrorDto<ContratoData> CxC_ContratoDetalle_Consultar(int codEmpresa, string cedula, string contrato)
        {
            return _bl.CxC_ContratoDetalle_Consultar(codEmpresa, cedula, contrato);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesConceptos_Consultar")] 
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesConceptos_Consultar(int codEmpresa, int orden, string concepto = "")
        {
            return _bl.CxC_CuentasCorreccionesConceptos_Consultar(codEmpresa, orden, concepto);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesPagadores_Consultar")] 
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesPagadores_Consultar(int codEmpresa, int orden, bool mCntPagadorAbierto, string cedula, string contrato,string pagadorCedula = "")
        {
            return _bl.CxC_CuentasCorreccionesPagadores_Consultar(codEmpresa, orden, mCntPagadorAbierto, cedula, pagadorCedula, contrato);
        }

        [Authorize]  
        [HttpGet("CxC_CuentasCorrecciones_Consultar")]
        public ErrorDto<CuentaPorCobrarData> CxC_CuentasCorrecciones_Consultar(int CodEmpresa, int operacion)
        {
            return _bl.CxC_CuentasCorrecciones_Consultar(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesClientesNombre_Consultar")]
        public ErrorDto<string> CxC_CuentasCorreccionesClientesNombre_Consultar(int CodEmpresa, string cedula)
        {
            return _bl.CxC_CuentasCorreccionesClientesNombre_Consultar(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesClientes_Listado")] 
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesClientes_Listado(int CodEmpresa)
        {
            return _bl.CxC_CuentasCorreccionesClientes_Listado(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesConceptos_Listado")]  
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesConceptos_Listado(int CodEmpresa)
        {
            return _bl.CxC_CuentasCorreccionesConceptos_Listado(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesContratos_Listado")] 
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesContratos_Listado(int CodEmpresa, string cedula, string concepto)
        {
            return _bl.CxC_CuentasCorreccionesContratos_Listado(CodEmpresa, cedula, concepto);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesPagadores_Listado")] 
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesPagadores_Listado(int CodEmpresa, bool mCntPagadorAbierto, string cedula, string contrato)
        {
            return _bl.CxC_CuentasCorreccionesPagadores_Listado(CodEmpresa, mCntPagadorAbierto, cedula, contrato);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesAutorizados_Listado")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesAutorizados_Listado(int CodEmpresa, string cedula)
        {
            return _bl.CxC_CuentasCorreccionesAutorizados_Listado(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CxC_CuentasCorreccionesConceptosDatos_Obtener")] 
        public ErrorDto<ConceptosData> CxC_CuentasCorreccionesConceptosDatos_Obtener(int CodEmpresa, string concepto)
        {

            return _bl.CxC_CuentasCorreccionesConceptosDatos_Obtener(CodEmpresa, concepto);
        }

        [Authorize]
        [HttpPost("CxC_CuentasCorrecciones_Actualizar")]
        public ErrorDto CxC_CuentasCorrecciones_Actualizar(int codEmpresa, string usuario, [FromBody] CuentaPorCobrarData datos)
        {
            return _bl.CxC_CuentasCorrecciones_Actualizar(codEmpresa, usuario, datos);
        }

    }
}