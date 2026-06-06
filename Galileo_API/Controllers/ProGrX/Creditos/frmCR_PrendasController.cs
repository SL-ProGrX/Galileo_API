using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPrendasController : ControllerBase
    {
        private readonly FrmCrPrendasBl _bl;

        public FrmCrPrendasController(IConfiguration config)
        {
            _bl = new FrmCrPrendasBl(config);
        }

        [HttpGet("CR_Prendas_Obtener")]
        public ErrorDto<List<CrPrendaListaData>> CR_Prendas_Obtener(
            int codEmpresa,
            long operacion,
            string expediente = "")
        {
            return _bl.CR_Prendas_Obtener(codEmpresa, operacion, expediente);
        }

        [HttpGet("CR_Prendas_ObtenerDetalle")]
        public ErrorDto<CrPrendaDetalleData> CR_Prendas_ObtenerDetalle(int codEmpresa, long prendaId)
        {
            return _bl.CR_Prendas_ObtenerDetalle(codEmpresa, prendaId);
        }

        [HttpGet("CR_Prendas_TiposActivos")]
        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_TiposActivos(int codEmpresa)
        {
            return _bl.CR_Prendas_TiposActivos(codEmpresa);
        }

        [HttpGet("CR_Prendas_CatalogoLista")]
        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_CatalogoLista(int codEmpresa, string tipoCatalogo)
        {
            return _bl.CR_Prendas_CatalogoLista(codEmpresa, tipoCatalogo);
        }

        [HttpGet("CR_Prendas_UnidadesLista")]
        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_UnidadesLista(int codEmpresa, string aplicacion)
        {
            return _bl.CR_Prendas_UnidadesLista(codEmpresa, aplicacion);
        }

        [HttpGet("CR_Prendas_ParentescosLista")]
        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_ParentescosLista(int codEmpresa)
        {
            return _bl.CR_Prendas_ParentescosLista(codEmpresa);
        }

        [HttpGet("CR_Prendas_TiposIdentificacionLista")]
        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_TiposIdentificacionLista(int codEmpresa)
        {
            return _bl.CR_Prendas_TiposIdentificacionLista(codEmpresa);
        }

        [HttpGet("CR_Prendas_AnotacionesLista")]
        public ErrorDto<List<CrPrendaAnotacionData>> CR_Prendas_AnotacionesLista(int codEmpresa, long prendaId)
        {
            return _bl.CR_Prendas_AnotacionesLista(codEmpresa, prendaId);
        }

        [HttpGet("CR_Prendas_PolizasList")]
        public ErrorDto<List<CrPrendaPolizaCoberturaData>> CR_Prendas_PolizasList(
            int codEmpresa,
            string tipoPrenda,
            long prendaId)
        {
            return _bl.CR_Prendas_PolizasList(codEmpresa, tipoPrenda, prendaId);
        }

        [HttpGet("CR_Prendas_AvaluosLista")]
        public ErrorDto<List<CrPrendaHistoricoAvaluoData>> CR_Prendas_AvaluosLista(int codEmpresa, long prendaId)
        {
            return _bl.CR_Prendas_AvaluosLista(codEmpresa, prendaId);
        }

        [HttpPost("CR_Prendas_AvaluoGuardar")]
        public ErrorDto<string> CR_Prendas_AvaluoGuardar(
            int codEmpresa,
            [FromBody] CrPrendaAvaluoGuardarRequest request)
        {
            return _bl.CR_Prendas_AvaluoGuardar(codEmpresa, request);
        }

        [HttpPost("CR_Prendas_NotariadoGuardar")]
        public ErrorDto<string> CR_Prendas_NotariadoGuardar(
            int codEmpresa,
            [FromBody] CrPrendaNotariadoGuardarRequest request)
        {
            return _bl.CR_Prendas_NotariadoGuardar(codEmpresa, request);
        }

        [HttpPost("CR_Prendas_NotaGuardar")]
        public ErrorDto<string> CR_Prendas_NotaGuardar(
            int codEmpresa,
            [FromBody] CrPrendaNotaGuardarRequest request)
        {
            return _bl.CR_Prendas_NotaGuardar(codEmpresa, request);
        }

        [HttpPost("CR_Prendas_PolizaCoberturaGuardar")]
        public ErrorDto<string> CR_Prendas_PolizaCoberturaGuardar(
            int codEmpresa,
            [FromBody] CrPrendaPolizaCoberturaGuardarRequest request)
        {
            return _bl.CR_Prendas_PolizaCoberturaGuardar(codEmpresa, request);
        }

        [HttpGet("CR_Prendas_PolizasExternasLista")]
        public ErrorDto<List<CrPrendaHistoricoPolizaData>> CR_Prendas_PolizasExternasLista(int codEmpresa, long prendaId)
        {
            return _bl.CR_Prendas_PolizasExternasLista(codEmpresa, prendaId);
        }

        [HttpGet("CR_Prendas_PolizaExternaLoad")]
        public ErrorDto<CrPrendaDetalleData> CR_Prendas_PolizaExternaLoad(int codEmpresa, long prendaId, int polizaExtId)
        {
            return _bl.CR_Prendas_PolizaExternaLoad(codEmpresa, prendaId, polizaExtId);
        }

        [HttpPost("CR_Prendas_Guardar")]
        public ErrorDto<long> CR_Prendas_Guardar(
            int codEmpresa,
            [FromBody] CrPrendaGuardarCompletaRequest request)
        {
            return _bl.CR_Prendas_Guardar(codEmpresa, request);
        }

        [HttpPost("CR_Prendas_PolizaExternaGuardar")]
        public ErrorDto<string> CR_Prendas_PolizaExternaGuardar(
            int codEmpresa,
            [FromBody] CrPrendaPolizaExternaGuardarRequest request)
        {
            return _bl.CR_Prendas_PolizaExternaGuardar(codEmpresa, request);
        }

        [HttpDelete("CR_Prendas_Eliminar")]
        public ErrorDto CR_Prendas_Eliminar(int codEmpresa, [FromBody] CrPrendasEliminarRequest request)
        {
            return _bl.CR_Prendas_Eliminar(codEmpresa, request);
        }
    }
}
