using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoRevisionesTagController : ControllerBase
    {
        private readonly FrmCrSeguimientoRevisionesTagBL _bl;

        public FrmCrSeguimientoRevisionesTagController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoRevisionesTagBL(config);
        }

        [HttpGet("Cr_SeguimientoRevisionesTag_Bancos_Obtener")]
        public ErrorDto<List<CrSeguimientoRevisionesTagBancoRow>> Cr_SeguimientoRevisionesTag_Bancos_Obtener(
            int codEmpresa)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Bancos_Obtener(codEmpresa);
        }

        [HttpGet("Cr_SeguimientoRevisionesTag_Etiquetas_Obtener")]
        public ErrorDto<List<CrSeguimientoRevisionesTagEtiquetaRow>> Cr_SeguimientoRevisionesTag_Etiquetas_Obtener(
            int codEmpresa,
            string usuario)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Etiquetas_Obtener(codEmpresa, usuario);
        }

        [HttpPost("Cr_SeguimientoRevisionesTag_Operaciones_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagOperacionesResponse> Cr_SeguimientoRevisionesTag_Operaciones_Obtener(
            int codEmpresa,
            [FromBody] CrSeguimientoRevisionesTagOperacionesFiltrosRequest request)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Operaciones_Obtener(codEmpresa, request);
        }

        [HttpPost("Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagDetalleCreditoResponse> Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(
            int codEmpresa,
            [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _bl.Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(codEmpresa, request);
        }

        //[HttpPost("Cr_SeguimientoRevisionesTag_Patrimonio_Obtener")]
        //public ErrorDto<CrSeguimientoRevisionesTagPatrimonioResponse> Cr_SeguimientoRevisionesTag_Patrimonio_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_Patrimonio_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_Deudas_Obtener")]
        //public ErrorDto<List<CrSeguimientoRevisionesTagDeudaRow>> Cr_SeguimientoRevisionesTag_Deudas_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_Deudas_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_Fianzas_Obtener")]
        //public ErrorDto<List<CrSeguimientoRevisionesTagFianzaRow>> Cr_SeguimientoRevisionesTag_Fianzas_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_Fianzas_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_Refundiciones_Obtener")]
        //public ErrorDto<List<CrSeguimientoRevisionesTagRefundicionRow>> Cr_SeguimientoRevisionesTag_Refundiciones_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_Refundiciones_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_Desembolsos_Obtener")]
        //public ErrorDto<List<CrSeguimientoRevisionesTagDesembolsoRow>> Cr_SeguimientoRevisionesTag_Desembolsos_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_Desembolsos_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener")]
        //public ErrorDto<CrSeguimientoRevisionesTagFiadorResponse> Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagFiadorRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(codEmpresa, request);
        //}

        //[HttpPost("Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener")]
        //public ErrorDto<List<CrSeguimientoRevisionesTagClasificacionRow>> Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener(
        //    int codEmpresa,
        //    [FromBody] CrSeguimientoRevisionesTagFiadorRequest request)
        //{
        //    return _bl.Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener(codEmpresa, request);
        //}

        [HttpPost("Cr_SeguimientoRevisionesTag_Seguimiento_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagSeguimientoResponse> Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(
            int codEmpresa,
            [FromBody] CrSeguimientoRevisionesTagSeguimientoRequest request)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(codEmpresa, request);
        }

        [HttpPost("Cr_SeguimientoRevisionesTag_Revision_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagRevisionResponse> Cr_SeguimientoRevisionesTag_Revision_Obtener(
            int codEmpresa,
            [FromBody] CrSeguimientoRevisionesTagDetalleRequest request)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Revision_Obtener(codEmpresa, request);
        }

        [HttpGet("Cr_SeguimientoRevisionesTag_NotaLargo_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagNotaLargoResponse> Cr_SeguimientoRevisionesTag_NotaLargo_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            return _bl.Cr_SeguimientoRevisionesTag_NotaLargo_Obtener(codEmpresa, tagCodigo);
        }

        [HttpGet("Cr_SeguimientoRevisionesTag_Aviso_Obtener")]
        public ErrorDto<CrSeguimientoRevisionesTagAvisoResponse> Cr_SeguimientoRevisionesTag_Aviso_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Aviso_Obtener(codEmpresa, tagCodigo);
        }

        [HttpPost("Cr_SeguimientoRevisionesTag_Aplicar")]
        public ErrorDto<CrSeguimientoRevisionesTagAplicarResponse> Cr_SeguimientoRevisionesTag_Aplicar(
            int codEmpresa,
            string usuario,
            [FromBody] CrSeguimientoRevisionesTagAplicarRequest request)
        {
            return _bl.Cr_SeguimientoRevisionesTag_Aplicar(codEmpresa, usuario, request);
        }
    }
}
