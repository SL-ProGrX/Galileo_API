using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrCreditosCargaLoteController : ControllerBase
    {
        private readonly FrmCrCreditosCargaLoteBL _bl;

        public FrmCrCreditosCargaLoteController(IConfiguration config)
        {
            _bl = new FrmCrCreditosCargaLoteBL(config);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_Cliente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_Cliente_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Destinos_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.CrCreditosCargaLote_Destinos_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_ConceptosDesembolso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ConceptosDesembolso_Obtener(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_ConceptosDesembolso_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_ObtenerDeductoras")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ObtenerDeductoras(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_ObtenerDeductoras(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_ObtenerFrecuenciaDeductora")]
        public ErrorDto<List<FrecuenciaReductora>> CrCreditosCargaLote_ObtenerFrecuenciaDeductora(int CodEmpresa, string CodInstitucion)
        {
            return _bl.CrCreditosCargaLote_ObtenerFrecuenciaDeductora(CodEmpresa, CodInstitucion);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_Banco_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CrCreditosCargaLote_Banco_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpDelete("CrCreditosCargaLote_Cargado_Eliminar")]
        public ErrorDto CrCreditosCargaLote_Cargado_Eliminar(int CodEmpresa, string Codigo, long Proceso)
        {
            return _bl.CrCreditosCargaLote_Cargado_Eliminar(CodEmpresa, Codigo, Proceso);
        }

        [Authorize]
        [HttpPost("CrCreditosCargaLote_Cargado_Insertar")]
        public ErrorDto CrCreditosCargaLote_Cargado_Insertar(int CodEmpresa, CrCreditosCargaLoteCargadoInsertarRequest request)
        {
            return _bl.CrCreditosCargaLote_Cargado_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CrCreditosCargaLote_Cargado_Revisado")]
        public ErrorDto<List<CrCreditosCargaLoteCargadoRevisadoResponse>> CrCreditosCargaLote_Cargado_Revisado(int CodEmpresa, CrCreditosCargaLoteCargadoRevisadoRequest request)
        {
            return _bl.CrCreditosCargaLote_Cargado_Revisado(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_ProveedorCxp_Obtener")]
        public ErrorDto<List<ProveedorCxpModel>> CrCreditosCargaLote_ProveedorCxp_Obtener(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_ProveedorCxp_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CrCreditosCargaLote_Procesa")]
        public ErrorDto CrCreditosCargaLote_Procesa(int CodEmpresa, CrCreditosCargaLoteProcesaRequest request)
        {
            return _bl.CrCreditosCargaLote_Procesa(CodEmpresa, request);
        }
    }
}
