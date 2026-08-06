using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCorreccionCreditosController : ControllerBase
    {
        private readonly FrmCrCorreccionCreditosBl _bl;

        public FrmCrCorreccionCreditosController(IConfiguration config)
            => _bl = new FrmCrCorreccionCreditosBl(config);

        [HttpGet("CR_CorreccionCreditos_Operacion_Obtener")]
        public ErrorDto<CrCorreccionCreditosConsultaResponse> CR_CorreccionCreditos_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario)
            => _bl.CR_CorreccionCreditos_Operacion_Obtener(codEmpresa, operacion, usuario);

        [HttpGet("CR_CorreccionCreditos_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CorreccionCreditos_Catalogo_Obtener(
            int codEmpresa,
            int movimiento,
            string codigo)
            => _bl.CR_CorreccionCreditos_Catalogo_Obtener(codEmpresa, movimiento, codigo);

        [HttpGet("CR_CorreccionCreditos_Detalle_Obtener")]
        public ErrorDto<List<CrCorreccionCreditosDetalleSeleccion>> CR_CorreccionCreditos_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int movimiento)
            => _bl.CR_CorreccionCreditos_Detalle_Obtener(codEmpresa, operacion, movimiento);

        [HttpGet("CR_CorreccionCreditos_Proceso_Obtener")]
        public ErrorDto<int> CR_CorreccionCreditos_Proceso_Obtener(
            int codEmpresa,
            int proceso,
            int direccion)
            => _bl.CR_CorreccionCreditos_Proceso_Obtener(codEmpresa, proceso, direccion);

        [HttpPut("CR_CorreccionCreditos_Cambio_Aplicar")]
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Cambio_Aplicar(
            int codEmpresa,
            CrCorreccionCreditosAplicarRequest request)
            => _bl.CR_CorreccionCreditos_Cambio_Aplicar(codEmpresa, request);

        [HttpDelete("CR_CorreccionCreditos_Formalizacion_Anular")]
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Formalizacion_Anular(
            int codEmpresa,
            CrCorreccionCreditosAnularRequest request)
            => _bl.CR_CorreccionCreditos_Formalizacion_Anular(codEmpresa, request);

        [HttpDelete("CR_CorreccionCreditos_Operacion_Excluir")]
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Operacion_Excluir(
            int codEmpresa,
            [FromBody] CrCorreccionCreditosExcluirRequest request)
            => _bl.CR_CorreccionCreditos_Operacion_Excluir(codEmpresa, request);
    }
}
