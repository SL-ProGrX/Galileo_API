using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizaProcRecepcionController : ControllerBase
    {
        private readonly FrmCrPolizaProcRecepcionBL _bl;

        public FrmCrPolizaProcRecepcionController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcRecepcionBL(config);
        }

        [HttpGet("Crd_PolizaProcRecepcion_Polizas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Polizas_Listar(int codEmpresa)
            => _bl.Crd_PolizaProcRecepcion_Polizas_Listar(codEmpresa);

        [HttpGet("Crd_PolizaProcRecepcion_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Unidades_Listar(int codEmpresa, int codContabilidad)
            => _bl.Crd_PolizaProcRecepcion_Unidades_Listar(codEmpresa, codContabilidad);

        [HttpGet("Crd_PolizaProcRecepcion_CentrosCosto_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_CentrosCosto_Listar(int codEmpresa, int codContabilidad, string codUnidad)
            => _bl.Crd_PolizaProcRecepcion_CentrosCosto_Listar(codEmpresa, codContabilidad, codUnidad);

        [HttpGet("Crd_PolizaProcRecepcion_Divisas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Divisas_Listar(int codEmpresa, int codContabilidad)
            => _bl.Crd_PolizaProcRecepcion_Divisas_Listar(codEmpresa, codContabilidad);

        [HttpGet("Crd_PolizaProcRecepcion_DivisaLocal_Obtener")]
        public ErrorDto<DropDownListaGenericaModel?> Crd_PolizaProcRecepcion_DivisaLocal_Obtener(int codEmpresa, int codContabilidad)
            => _bl.Crd_PolizaProcRecepcion_DivisaLocal_Obtener(codEmpresa, codContabilidad);

        [HttpGet("Crd_PolizaProcRecepcion_TipoCambio_Obtener")]
        public ErrorDto<decimal> Crd_PolizaProcRecepcion_TipoCambio_Obtener(int codEmpresa, int codContabilidad, string divisa)
            => _bl.Crd_PolizaProcRecepcion_TipoCambio_Obtener(codEmpresa, codContabilidad, divisa);

        [HttpGet("Crd_PolizaProcRecepcion_Poliza_Datos_Obtener")]
        public ErrorDto<PolizaDatos?> Crd_PolizaProcRecepcion_Poliza_Datos_Obtener(int codEmpresa, string codPoliza)
            => _bl.Crd_PolizaProcRecepcion_Poliza_Datos_Obtener(codEmpresa, codPoliza);

        [HttpGet("Crd_PolizaProcRecepcion_Corte_Consultar")]
        public ErrorDto<PolizaAseguradoraCorteConsulta?> Crd_PolizaProcRecepcion_Corte_Consultar(int codEmpresa, DateTime corte, string codPoliza)
            => _bl.Crd_PolizaProcRecepcion_Corte_Consultar(codEmpresa, corte, codPoliza);

        [HttpGet("Crd_PolizaProcRecepcion_Corte_Detalle_Consultar")]
        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> Crd_PolizaProcRecepcion_Corte_Detalle_Consultar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
            => _bl.Crd_PolizaProcRecepcion_Corte_Detalle_Consultar(codEmpresa, corte, codPoliza, idFactura);

        [HttpGet("Crd_PolizaProcRecepcion_Corte_Validar")]
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Corte_Validar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
            => _bl.Crd_PolizaProcRecepcion_Corte_Validar(codEmpresa, corte, codPoliza, idFactura);

        [ValidateAntiForgeryToken]
        [HttpPost("Crd_PolizaProcRecepcion_Archivo_Cargar")]
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Archivo_Cargar(int codEmpresa, string usuario, [FromBody] PolizaArchivoCargarRequest request)
            => _bl.Crd_PolizaProcRecepcion_Archivo_Cargar(codEmpresa, usuario, request);

        // POST (no DELETE): mismo estandar que Bancos_Cargado y Proc_Prevista para eliminaciones por lote con cuerpo.
        [ValidateAntiForgeryToken]
        [HttpPost("Crd_PolizaProcRecepcion_Detalle_Eliminar")]
        public ErrorDto<string> Crd_PolizaProcRecepcion_Detalle_Eliminar(int codEmpresa, string usuario, [FromBody] PolizaDetalleEliminarRequest request)
            => _bl.Crd_PolizaProcRecepcion_Detalle_Eliminar(codEmpresa, usuario, request);

        [ValidateAntiForgeryToken]
        [HttpPost("Crd_PolizaProcRecepcion_Factura_Generar")]
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Factura_Generar(int codEmpresa, string usuario, [FromBody] PolizaAseguradoraCorteData datos)
            => _bl.Crd_PolizaProcRecepcion_Factura_Generar(codEmpresa, usuario, datos);
    }
}
